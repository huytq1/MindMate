﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindMate.Model;
using MindMate.Serialization;
using MindMate.View.MapControls;
using MindMate.Tests.Stubs;
using System.Windows.Forms;
using MindMate.MetaModel;
using MindMate.Controller;
using System.IO;
using XnaFan.ImageComparison;
using System.Drawing;

namespace MindMate.Tests.IntegrationTest
{
    [TestClass]
    public class MapCtrlTests
    {
        public const bool SAVE_ACTUAL_IMAGE = true;
        public const bool CONDUCT_INTERMEDIATE_TESTS = true;

        [TestMethod]
        public void MapCtrl_Test()
        {
            string xmlString = System.IO.File.ReadAllText(@"Resources\Feature Display.mm");

            MapTree tree = new MapTree();
            new MindMapSerializer().Deserialize(xmlString, tree);

            tree.SelectedNodes.Add(tree.RootNode, false);

            var form = new System.Windows.Forms.Form();
            MetaModel.MetaModel.Initialize();
            MetaModel.MetaModel.Instance.MapEditorBackColor = Color.White;
            MetaModel.MetaModel.Instance.NoteEditorBackColor = Color.White;
            MapCtrl mapCtrl = new MapCtrl(new MapView(tree), new MainCtrlStub(form));
            form.Controls.Add(mapCtrl.MapView.Canvas);

            tree.TurnOnChangeManager();

            // folding test
            mapCtrl.AppendNodeAndEdit();
            mapCtrl.MapView.NodeTextEditor.EndNodeEdit(true, true);
            mapCtrl.UpdateNodeText(tree.RootNode.LastChild, "Test Folding");
            mapCtrl.AppendChildNode(tree.RootNode.LastChild);
            mapCtrl.AppendChildNode(tree.RootNode.LastChild);
            mapCtrl.AppendChildNode(tree.RootNode.LastChild);
            mapCtrl.SelectNodeRightOrUnfold();
            mapCtrl.ToggleNode();

            // delete test
            mapCtrl.SelectNodeAbove();
            mapCtrl.DeleteSelectedNodes();

            // move up
            mapCtrl.MoveNodeUp();

            // move right
            mapCtrl.SelectNodeBelow();
            for (int i = 0; i < 20; i++) mapCtrl.MoveNodeUp();

            //*****
            if (CONDUCT_INTERMEDIATE_TESTS) ImageTest(mapCtrl.MapView, "MapCtrl1");

            // move down     
            mapCtrl.SelectNodeRightOrUnfold();
            for (int i = 0; i < 5; i++) mapCtrl.SelectNodeAbove();
            for (int i = 0; i < 5; i++) mapCtrl.MoveNodeDown();

            // move up
            mapCtrl.SelectNodeAbove();
            for (int i = 0; i < 5; i++) mapCtrl.MoveNodeUp();

            // move left
            mapCtrl.SelectNodeLeftOrUnfold();
            for (int i = 0; i < 20; i++) mapCtrl.MoveNodeDown();

            // select siblings above
            mapCtrl.SelectNodeLeftOrUnfold();
            for (int i = 0; i < 3; i++) mapCtrl.SelectNodeBelow();
            mapCtrl.SelectAllSiblingsAbove();
            mapCtrl.ToggleSelectedNodeBold();

            // select siblings below
            mapCtrl.SelectNodeRightOrUnfold();
            mapCtrl.SelectNodeLeftOrUnfold();
            for (int i = 0; i < 3; i++) mapCtrl.SelectNodeAbove();
            mapCtrl.SelectNodeBelow();
            mapCtrl.SelectAllSiblingsBelow();
            mapCtrl.ToggleSelectedNodeItalic();

            //*****
            if (CONDUCT_INTERMEDIATE_TESTS) ImageTest(mapCtrl.MapView, "MapCtrl2");

            // add icon
            mapCtrl.AppendIcon("clock");
            mapCtrl.AppendIcon("idea");

            // remove last icon
            mapCtrl.RemoveLastIcon();

            // remove all icon
            mapCtrl.SelectNodeRightOrUnfold();
            mapCtrl.SelectNodeLeftOrUnfold();
            for (int i = 0; i < 3; i++) mapCtrl.SelectNodeBelow();
            mapCtrl.RemoveAllIcon();

            //*****
            if (CONDUCT_INTERMEDIATE_TESTS) ImageTest(mapCtrl.MapView, "MapCtrl3");

            mapCtrl.AppendNodeAndEdit();
            mapCtrl.MapView.NodeTextEditor.EndNodeEdit(true, true);
            mapCtrl.UpdateNodeText(tree.SelectedNodes.First, "Format Test");

            mapCtrl.ChangeLineColor();
            mapCtrl.ChangeLinePattern(System.Drawing.Drawing2D.DashStyle.Dash);
            mapCtrl.ChangeLineWidth(2);
            mapCtrl.ChangeFont();

            //*****
            if (CONDUCT_INTERMEDIATE_TESTS) ImageTest(mapCtrl.MapView, "MapCtrl4");


            mapCtrl.SelectNodeRightOrUnfold();
            mapCtrl.AppendSiblingNodeAndEdit();
            mapCtrl.MapView.NodeTextEditor.EndNodeEdit(true, true);
            mapCtrl.UpdateNodeText(tree.SelectedNodes.First, "Node Color");

            // change node color
            mapCtrl.AppendChildNode(tree.SelectedNodes.First);
            mapCtrl.UpdateNodeText(tree.SelectedNodes.First, "Node Color");
            mapCtrl.ChangeTextColorByPicker();

            // unfolding
            mapCtrl.SelectNodeRightOrUnfold();
            mapCtrl.ToggleNode();
            mapCtrl.SelectNodeLeftOrUnfold();

            // change background color
            mapCtrl.AppendChildNodeAndEdit();
            mapCtrl.MapView.NodeTextEditor.EndNodeEdit(true, true);
            mapCtrl.UpdateNodeText(tree.SelectedNodes.First, "Background Color");
            mapCtrl.ChangeBackColorByPicker();
            mapCtrl.SelectNodeRightOrUnfold();

            //*****
            ImageTest(mapCtrl.MapView, "MapCtrl5");


            VerifySerializeDeserialize(mapCtrl);
            form.Dispose();
            //form.Close();                 

        }

        /// <summary>
        /// Confirming that MapView look doesn't change by serializing and deserializing
        /// </summary>
        /// <param name="mapCtrl"></param>
        private void VerifySerializeDeserialize(MapCtrl mapCtrl)
        {
            //1. save MapView as image
            mapCtrl.MapView.SelectedNodes.Add(mapCtrl.MapView.Tree.RootNode, false);
            var refImage = mapCtrl.MapView.DrawToBitmap();

            //2. serialize
            var s = new MindMapSerializer();
            MemoryStream stream = new MemoryStream();
            s.Serialize(stream, mapCtrl.MapView.Tree);
            stream.Position = 0;
            string generatedText = new StreamReader(stream).ReadToEnd();
            stream.Close();

            //3. deserialize
            MapTree newTree = new MapTree();
            s.Deserialize(generatedText, newTree);
            newTree.SelectedNodes.Add(newTree.RootNode, false);
            mapCtrl = new MapCtrl(new MapView(newTree), new MainCtrlStub(new Form()));

            //4. save new MapView image and compare
            var image = mapCtrl.MapView.DrawToBitmap();
            if (SAVE_ACTUAL_IMAGE) refImage.Save(@"Resources\MapCtrl_BeforeSerialization.png");
            if (SAVE_ACTUAL_IMAGE) image.Save(@"Resources\MapCtrl_AfterDeseriallization.png");
            Assert.AreEqual(0.0f, refImage.PercentageDifference(image, 0), "MapCtrl Test: Final image doesn't match.");

            image.Dispose();
            refImage.Dispose();
        }

        private void ImageTest(MapView view, string imageName)
        {
            using (var image = view.DrawToBitmap())
            {
                if (SAVE_ACTUAL_IMAGE) image.Save(@"Resources\" + imageName + " - Actual.png");
                using (var refImage = (Bitmap)Bitmap.FromFile(@"Resources\" + imageName + ".png"))
                {
                    Assert.AreEqual(0.0f, image.PercentageDifference(refImage, 0), "MapCtrlTest failed for image:" + imageName + ".png");
                }
            }
        }
    }
}
