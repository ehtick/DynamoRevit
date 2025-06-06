﻿using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using Dynamo.Nodes;
using Autodesk.DesignScript.Geometry;
using CoreNodeModels.Input;
using NUnit.Framework;

using RevitServices.Persistence;

using RevitTestServices;
using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class CurveTests : RevitSystemTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CurveByPoints()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Curve\CurveByPoints.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            //cerate some points and wire them
            //to the selections
            ReferencePoint p1, p2, p3, p4;

            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document))
            {
                trans.Start("Create reference points for testing.");

                p1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ(1, 5, 12));
                p2 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ(5, 1, 12));
                p3 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ(12, 1, 5));
                p4 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ(5, 12, 1));

                trans.Commit();
            }

            var ptSelectNodes = ViewModel.Model.CurrentWorkspace.Nodes.Where(x => x is DSModelElementSelection);
            if (!ptSelectNodes.Any())
                Assert.Fail("Could not find point selection nodes in dynamo graph.");

            ((DSModelElementSelection)ptSelectNodes.ElementAt(0)).UpdateSelection(new[] { p1 }); ;
            ((DSModelElementSelection)ptSelectNodes.ElementAt(1)).UpdateSelection(new[] { p2 });
            ((DSModelElementSelection)ptSelectNodes.ElementAt(2)).UpdateSelection(new[] { p3 });
            ((DSModelElementSelection)ptSelectNodes.ElementAt(3)).UpdateSelection(new[] { p4 });

            RunCurrentModel();

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsTrue(mc.IsReferenceLine);

            //now flip the switch for creating a reference curve
            var boolNode = ViewModel.Model.CurrentWorkspace.Nodes.Where(x => x is CoreNodeModels.Input.BoolSelector).First();

            ((CoreNodeModels.Input.BasicInteractive<bool>)boolNode).Value = false;

            RunCurrentModel();

            Assert.AreEqual(fec.ToElements().Count(), 1);

            mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsFalse(mc.IsReferenceLine);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CurveLoop()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Curve\CurveLoop.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            AssertNoDummyNodes();

            var model = ViewModel.Model;
            Assert.AreEqual(19, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(31, model.CurrentWorkspace.Connectors.Count());

            //check PolyCurve.ByThickeningCurve
            var polyCurveId = "07fd72a70bbf48408b3ab1e7778b82b5";
            AssertPreviewCount(polyCurveId, 4);
            for (int i = 0; i < 4; i++)
            {
                var polyCurve = GetPreviewValueAtIndex(polyCurveId, i) as PolyCurve;
                Assert.IsNotNull(polyCurve);
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CurvebyPointsArc()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Curve\CurvebyPointsArc.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            //check Arc.ByThreePoints
            var arcID = "e52e6a42-7cf7-41f6-b8df-2b882992167d";
            var arc = GetPreviewValue(arcID) as Autodesk.DesignScript.Geometry.Arc;
            Assert.IsNotNull(arc);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void OffsetCurve()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Curve\OffsetCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            RunCurrentModel();
            AssertNoDummyNodes();
            var model = ViewModel.Model;
            Assert.AreEqual(28, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(38, model.CurrentWorkspace.Connectors.Count());

            //check curve.Offset
            var curve1ID = "dfee4fa337b5476995912e653d395045";
            var curve2ID = "21cb36fafb504bf8932d770a8432c7b2";
            var curve3ID = "4a7e6ca158644197acbc014823aa32ec";
            var curve1 = GetPreviewValue(curve1ID) as Autodesk.DesignScript.Geometry.Curve;
            var curve2 = GetPreviewValue(curve2ID) as Autodesk.DesignScript.Geometry.Curve;
            var curve3 = GetPreviewValue(curve3ID) as Autodesk.DesignScript.Geometry.Curve;
            Assert.IsNotNull(curve1);
            Assert.IsNotNull(curve2);
            Assert.IsNotNull(curve3);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ThickenCurve()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Curve\ThickenCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            AssertNoDummyNodes();
            var model = ViewModel.Model;
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(29, model.CurrentWorkspace.Connectors.Count());

            //check ThickenCurve
            var curveId = "f89ac92302b448d8bdc512d6e95e1a70";
            AssertPreviewCount(curveId, 4);
            for (int i = 0; i < 4; i++)
            {
                var curve = GetPreviewValueAtIndex(curveId, i) as PolyCurve;
                Assert.IsNotNull(curve);
            }
        }


        [Test]
        [TestModel(@".\empty.rfa")]
        public void CurveByPointsByLineNode()
        {
            //this sample creates a geometric line
            //then creates a curve by points from that line

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Curve\CurveByPointsByLine.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(1, fec.ToElements().Count());

            //now change one of the number inputs and rerun
            //verify that there are still only two reference points in
            //the model
            var node = ViewModel.Model.CurrentWorkspace.Nodes.OfType<DoubleInput>().First();
            node.Value = "12.0";

            RunCurrentModel();

            fec = null;
            fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(CurveElement));
            Assert.AreEqual(1, fec.ToElements().Count);
        }

        /*
        [Test]
        public void ClosedCurve()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(workingDirectory, @".\Curve\ClosedCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);
            model.Open(testPath);
           
            RunCurrentModel();
            
            var extrudeNode = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x is CreateExtrusionGeometry);
            var result = (Solid)VisualizationManager.GetDrawablesFromNode(extrudeNode).Values.First();
            double volumeMin = 3850;
            double volumeMax = 4050;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }
         * */

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CurvebyPointsEllipse()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Curve\CurvebyPointsEllipse.dyn");  //There are some obsolete node in dyn script.
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);
        }


        [Test]
        [TestModel(@".\empty.rfa")]
        public void CurvebyPoints_ByReferencePoints()
        {
            // This test is for testing the reference defect in
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6710
            // check whether CurveByPoints.ByReferencePoints does work.
            string samplePath = Path.Combine(workingDirectory, @".\Curve\CurvebyPoints_ByReferencePoints.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            RunCurrentModel();

            Assert.AreEqual(8, ViewModel.Model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(9, ViewModel.Model.CurrentWorkspace.Connectors.Count());

            //check CurveByPoints.ByReferencePoints
            var curveByPointsID = "94af7a17-0961-496a-86f7-53b458bafb58";
            var curveByPoints = GetPreviewValue(curveByPointsID) as Revit.Elements.CurveByPoints;
            Assert.IsNotNull(curveByPoints);
        }

    }
}