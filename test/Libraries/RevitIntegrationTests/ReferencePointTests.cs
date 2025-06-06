﻿using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using NUnit.Framework;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    public class ReferencePointTests : RevitSystemTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanCreateAndDeleteAReferencePoint()
        {
            using (var trans = new Transaction(DocumentManager.Instance.CurrentDBDocument, "CreateAndDeleteAreReferencePoint"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                ReferencePoint rp = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ());

                //make a filter for reference points.
                ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                fec.WherePasses(ef);
                Assert.AreEqual(1, fec.ToElements().Count());

                DocumentManager.Instance.CurrentDBDocument.Delete(rp.Id);

                trans.Commit();
                Assert.AreEqual(0, fec.ToElements().Count());
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ReferencePoint()
        {
            string testPath = Path.Combine(workingDirectory, @".\ReferencePoint\ReferencePoint.dyn");
            ViewModel.OpenCommand.Execute(testPath);
            
            Assert.AreEqual(3, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            RunCurrentModel();

            //Ensure that the reference point is created
            var rpFilter = new ElementClassFilter(typeof(ReferencePoint));

            var referencePointNode = GetPreviewValue("25456637-2521-4245-900a-2272c4df3581");
            var expectedResult = "Reference Point: Location=(X=0, Y=0, Z=0)";
            Assert.AreEqual(expectedResult, referencePointNode.ToString());
        }
    }
}
