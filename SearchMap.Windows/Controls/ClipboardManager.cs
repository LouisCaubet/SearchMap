﻿using Newtonsoft.Json;
using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using SearchMapCore.Serialization;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SearchMap.Windows.Controls {

    static class ClipboardManager {

        // Formatting
        const string WEBNODE_FORMAT = "SearchMap_WebNode";

        // Adding to Clipboard
        public static void AddWebNode(WebNode node) {

            SerializableWebNode serializable = new SerializableWebNode(node);

            string serialized = JsonConvert.SerializeObject(serializable, Formatting.Indented,
                                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            Clipboard.Clear();
            Clipboard.SetData(WEBNODE_FORMAT, serialized);

        }

        public static void CopyToClipboard(UserControl control, bool cut = false) {

            if (control == null) return;

            if(control.GetType() == typeof(WebNodeControl)) {

                AddWebNode(((WebNodeControl)control).Node);
                if(cut) {
                    SearchMapCore.SearchMapCore.Graph.DeleteNode(((WebNodeControl)control).Node.Id);
                }

                SearchMapCore.SearchMapCore.Logger.Debug("Successfully added selected node to clipboard");

            }

        }

        // Pasting from Clipboard
        public static void Paste(Point? point = null) {

            if (Clipboard.ContainsData(WEBNODE_FORMAT)) {

                try {

                    var node = JsonConvert.DeserializeObject<SerializableWebNode>(
                        (string)Clipboard.GetData(WEBNODE_FORMAT));

                    // Regenerate new node to get Unique Id
                    WebNode newnode = new WebNode(SearchMapCore.SearchMapCore.Graph, node);
                    // New node is rendered by constructor (see WebNode.cs)

                    if (point.HasValue) {
                        newnode.MoveTo(MainWindow.Window.ConvertToLocation(point.Value));
                    }

                }
                catch (InvalidCastException exc) {

                    SearchMapCore.SearchMapCore.Logger.Error("Clipboard contains invalid data with format " + WEBNODE_FORMAT);
                    SearchMapCore.SearchMapCore.Logger.Error(exc.Message);
                    SearchMapCore.SearchMapCore.Logger.Error(exc.StackTrace);

                    Clipboard.Clear();
                    SearchMapCore.SearchMapCore.Logger.Info("Clipboard was cleared to remove invalid data.");

                }

            }

        }

    }

}