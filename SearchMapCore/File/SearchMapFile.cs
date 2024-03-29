﻿using Ionic.Zip;
using Newtonsoft.Json;
using SearchMapCore.Undoing;
using System;
using System.IO;
using System.Text;

namespace SearchMapCore.File {

    /// <summary>
    /// Manages interactions between SearchMap and the zip file representing a SearchMap Project.
    /// </summary>
    public sealed class SearchMapFile {

        string Path { get; set; }

        public Graph.Graph Graph { get; private set; }

        /// <summary>
        /// Opens the search map project from a path. If the project does not exist, it will be created.
        /// </summary>
        /// <param name="path"></param>
        public SearchMapFile(string path) {

            Path = path;
            Graph = OpenGraph();

        }

        /// <summary>
        /// Creates a new search map project with the given graph.
        /// Path must be an unexisting file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="graph"></param>
        public SearchMapFile(string path, Graph.Graph graph) {

            Graph = graph;
            Path = path;

            // Create empty graph.json file.
            using (ZipFile Zip = new ZipFile(Path, Encoding.UTF8)) {
                Zip.AddEntry("graph.json", new MemoryStream());
                Zip.Save();
            }

            SaveGraph();

        }

        /// <summary>
        /// Reads and deserializes the graph object from the file.
        /// </summary>
        /// <returns></returns>
        internal Graph.Graph OpenGraph() {

            using (ZipFile Zip = new ZipFile(Path, Encoding.UTF8)) {

                if (Zip.EntryFileNames.Contains("graph.json")) {

                    var entry = Zip["graph.json"];

                    // Unzip entry and read contents to string
                    MemoryStream stream = new MemoryStream();
                    entry.Extract(stream);
                    stream.Position = 0;

                    string json = null;
                    using (StreamReader reader = new StreamReader(stream)) {
                        json = reader.ReadToEnd();
                    }

                    // Deserialize graph
                    var snapshot = JsonConvert.DeserializeObject<Snapshot>(json);

                    // We are using the Snapshot class to create a new graph with the contents of the saved snapshot.
                    var graph = new Graph.Graph();

                    // Renderer needs to be set to revert to a snapshot.
                    graph.Renderer = SearchMapCore.Renderer;

                    snapshot.Graph = graph;
                    snapshot.Revert();

                    // Due to issues when deserializing abstract classes, we cannot directly serialize the graph, 
                    // but need to go through a Snapshot.

                    SearchMapCore.Logger.Info("Graph has been opened from file " + Path);

                    return graph;

                }
                else {

                    SearchMapCore.Logger.Info("Selected zip file does not contain definition for graph. Creating empty graph.");

                    var graph = new Graph.Graph();
                    Snapshot snapshot = new Snapshot(graph);

                    string json = JsonConvert.SerializeObject(snapshot, Formatting.Indented,
                            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                    // Write json to MemoryStream
                    using (MemoryStream stream = new MemoryStream()) {
                        var sw = new StreamWriter(stream, Encoding.UTF8);
                        try {

                            sw.Write(json);
                            sw.Flush();
                            stream.Seek(0, SeekOrigin.Begin);

                            // Add stream to zip
                            Zip.AddEntry("graph.json", stream);
                            Zip.Save(Path);

                        }
                        finally {
                            sw.Dispose();
                        }

                    }

                    return graph;

                }

            }

        }

        /// <summary>
        /// Saves the changes made to the Graph.
        /// </summary>
        public void SaveGraph() {

            string json = JsonConvert.SerializeObject(new Snapshot(Graph), Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // Write json to MemoryStream
            using (MemoryStream stream = new MemoryStream()) {
                var sw = new StreamWriter(stream, Encoding.UTF8);
                try {

                    sw.Write(json);
                    sw.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    // Add stream to zip
                    using (ZipFile Zip = new ZipFile(Path, Encoding.UTF8)) {
                        Zip.UpdateEntry("graph.json", stream);
                        Zip.Save(Path);
                    }

                }
                finally {
                    sw.Dispose();
                }

            }

        }

        /// <summary>
        /// Reads the file with the given name from the file into a MemoryStream
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MemoryStream ReadFile(string name) {

            using(ZipFile Zip = new ZipFile(Path, Encoding.UTF8)) {

                try {
                    var entry = Zip[name];
                    var stream = new MemoryStream();
                    entry.Extract(stream);
                    return stream;
                }
                catch (Exception) {
                    throw new ArgumentException("Could not retrieve file with name " + name + " from project.");
                }

            }

        }

        /// <summary>
        /// Replaces the content of the file name with the content.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public void UpdateFile(string name, MemoryStream content) {

            using (ZipFile Zip = new ZipFile(Path, Encoding.UTF8)) {

                try {
                    Zip.UpdateEntry(name, content);
                    Zip.Save();
                }
                catch (Exception) {
                    throw new ArgumentException("Could not update file with name " + name + " from project.");
                }

            }

        }

        /// <summary>
        /// Writes the given stream to a new file with the given name in the archive.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public void AddFile(string name, MemoryStream content) {

            using (ZipFile Zip = new ZipFile(Path, Encoding.UTF8)) {
                Zip.AddEntry(name, content);
                Zip.Save();
            }

        }

        /// <summary>
        /// Removes the file with the given name from the archive.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveFile(string name) {

            using (ZipFile Zip = new ZipFile(Path, Encoding.UTF8)) {
                Zip.RemoveEntry(name);
                Zip.Save();
            }

        }

    }

}
