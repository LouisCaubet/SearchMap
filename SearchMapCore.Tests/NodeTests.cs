using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchMapCore.Tests {

    [TestClass]
    public class NodeTests {

        Graph.Graph graph;
        Node node;

        Node other_test_node;

        public NodeTests() {

            graph = new Graph.Graph();
            node = new WebNode(graph, new Uri("http://test.com"), "");
            other_test_node = new WebNode(graph, new Uri("http://test.com"), "");

        }

        [TestMethod]
        public void GetParent_Test() {

            Node expected = null;
            Assert.AreEqual(expected, node.GetParent());

            node.SetParent(other_test_node);
            expected = other_test_node;
            Assert.AreEqual(expected, node.GetParent());

            node.SetParent(null);

        }

        [TestMethod]
        public void GetChildren_Test() {

            Node[] expected = new Node[0];
            Console.WriteLine(node.GetChildren().ToString());
            Assert.AreEqual(node.GetChildren(), node.GetChildren());

            expected = new Node[1] { other_test_node };
            other_test_node.SetParent(node);
            Assert.AreEqual(expected, node.GetChildren());

            other_test_node.SetParent(null);
            expected = new Node[0];
            Assert.AreEqual(expected, node.GetChildren());

        }

        [TestMethod]
        public void GetSiblings_Test() {

            Node[] expected = new Node[0];
            Assert.AreEqual(expected, node.GetSiblings());

            node.AddSibling(other_test_node);
            expected = new Node[1] { other_test_node };
            Assert.AreEqual(expected, node.GetSiblings());

            node.RemoveSibling(other_test_node.Id);
            expected = new Node[0];
            Assert.AreEqual(expected, node.GetSiblings());

        }



    }
}
