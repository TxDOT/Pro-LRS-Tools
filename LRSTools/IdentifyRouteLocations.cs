using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace LRSTools
{
    internal class IdentifyRouteLocations : MapTool
    {
        public IdentifyRouteLocations()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            //System.Diagnostics.Trace.WriteLine("Tool Active");
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {

            var identifyResult = await QueuedTask.Run(() =>
            {
                var resultString = "";
                var compoundResult = new StringBuilder();
                string resultMeasure = "";

                var mv = MapView.Active;

                // Get features that intersect geometry object
                //var features = mv.GetFeatures(geometry);

                // layer definition for all layers
                var lyrs = mv.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                foreach (var lyr in lyrs)
                {
                    //var lyr = lyrs.FirstOrDefault(); //For use with single layer instead of for loop

                    //Get current selection (to revert later)
                    Selection RevertSelection = lyr.GetSelection();

                    //Select features that intersect geometry
                    mv.SelectFeatures(geometry);

                    //Create a list of all OIDs in this layer (for use with stacked or intersecting linework)
                    IReadOnlyList<long> LyrOids = lyr.GetSelection().GetObjectIDs();
                    foreach (long oid in LyrOids)
                    {
                        double PrevMinM, PrevMaxM;
                        double minM, maxM;

                        //Create Edit operation 
                        var splitFeatures = new EditOperation() { };

                        //long oid = lyr.GetSelection().GetObjectIDs().FirstOrDefault();

                        //Inspector for line min/max before split 
                        var PrevMinMaxInspector = new Inspector(false);
                        PrevMinMaxInspector.Load(lyr, oid);
                        var PrevMinMaxShape = PrevMinMaxInspector["SHAPE"] as Polyline;

                        //Error handling in case the user clicks on an area of the map with no linework
                        if (PrevMinMaxShape == null)
                        {

                            lyr.SetSelection(RevertSelection);
                            continue;
                            //return null;
                        }

                        //Get min/max of 'before' line and then split.  Put the min/max in a list to compare to line after split
                        GeometryEngine.Instance.GetMinMaxM(PrevMinMaxShape, out PrevMinM, out PrevMaxM);
                        List<double> prevMeasures = new List<double> { PrevMinM, PrevMaxM };
                        splitFeatures.Split(lyr, oid, geometry);
                        splitFeatures.Execute();

                        //Create another inspector for the line after the split has occurred
                        var MinMaxInspector = new Inspector(false);
                        MinMaxInspector.Load(lyr, oid);
                        var MinMaxShape = MinMaxInspector["SHAPE"] as Polyline;


                        if (MinMaxShape == null)
                        {

                            lyr.SetSelection(RevertSelection);
                            continue;
                            //return null;
                        }

                        //Get min/max of newly split line
                        GeometryEngine.Instance.GetMinMaxM(MinMaxShape, out minM, out maxM);

                        //Compare the min max of the split line to the min/max of the original line.  If the min or max matches that
                        //of the original line, it is not the ID value.
                        if (prevMeasures.Contains(maxM)) { resultMeasure = Convert.ToString(minM); }
                        else { resultMeasure = Convert.ToString(maxM); };
                        prevMeasures = null;

                        //Undo the split
                        splitFeatures.UndoAsync();

                        //Remove the 'redo' operation from the operation stack
                        var operationMgr = MapView.Active.Map.OperationManager;

                        //operationMgr.FindRedoOperations()
                        //RemoveRedoOperation();

                        //Revert the selection to what was selected before the identify was run
                        lyr.SetSelection(RevertSelection);

                        resultString = string.Format("LayerName: {0}{1}OID: {2}{3}Measure: {4}{5}Minimum Measure: {6}{7}Maximum Measure: {8}{9}{10}",
                        Convert.ToString(lyr.Name),
                        Environment.NewLine,
                        Convert.ToString(oid),
                        Environment.NewLine,
                        resultMeasure,
                        Environment.NewLine,
                        Convert.ToString(PrevMinM),
                        Environment.NewLine,
                        Convert.ToString(PrevMaxM),
                        Environment.NewLine,
                        Environment.NewLine
                        );

                        compoundResult.Append(resultString.ToString());


                    }

                }

                return compoundResult.ToString();
                //return resultString.ToString();

            });

            if (identifyResult != null)
            {
                MessageBox.Show(identifyResult);
            }
            else
            {
                MessageBox.Show("null");
            }

            return true;
        }
    }
}
