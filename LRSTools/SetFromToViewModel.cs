using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.Mapping.Events;


namespace LRSTools
{
    internal class SetFromToViewModel : DockPane
    {
        private const string _dockPaneID = "LRSTools_SetFromTo";

        //Icommand is bound to the ApplyMeasures button in the .xaml file
        public ICommand ApplyMeasures { get; set; }

        protected SetFromToViewModel() { 

        //Subscribe to selection change in the map
        MapSelectionChangedEvent.Subscribe(OnSelectionChanged);
          
            //Relay Command executes set from/to measures function
            ApplyMeasures = new RelayCommand(() =>
                {

            //Get selected layer
            var map = MapView.Active.Map;
            FeatureLayer featurelayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline);

            QueuedTask.Run(() =>
            {
                //Logic to check how many features are selected
                if (featurelayer == null)
                {
                    MessageBox.Show(string.Format("No Features Selected"));
                    return;
                };

                var featCount = featurelayer.GetSelection().GetCount();
                if (featCount > 1)
                {
                    MessageBox.Show(string.Format("Please select a single feature."));
                    return;
                };

                //Begin Edit operation
                var editOp = new EditOperation
                {
                    Name = "Set From To"
                };

                //Get selection and initiate cursor object
                var cursor = featurelayer.GetSelection().Search();

                while (cursor.MoveNext())
                {
                    var feature = cursor.Current as Feature;
                    var originalLine = feature.GetShape() as Polyline;

                    //This is the function that actually sets the measures and interpolates
                    Polyline outPolyline = GeometryEngine.Instance.SetAndInterpolateMsBetween(
                        originalLine,
                         Math.Round(Convert.ToDouble(_fromMeasure), 3),
                         Math.Round(Convert.ToDouble(_toMeasure), 3)
                        ) as Polyline;

                    //Use original polyline object and replace it with the new polyline object
                    editOp.Modify(featurelayer, feature.GetObjectID(), outPolyline);

                }

                //Execute edit operation and save the edits
                editOp.Execute();
                Project.Current.SaveEditsAsync();

            });

        });

        }
    /// Show the DockPane.
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        //Get the min/max values for the selected polyline and populate the dockpane with those values
        private void OnSelectionChanged(MapSelectionChangedEventArgs args)
        {
            //new instance for active map and feature layer
            var Mmap = MapView.Active.Map;
            FeatureLayer Mfeaturelayer = Mmap.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline);

            QueuedTask.Run(() =>
            {
                //logic to check if only one feature is selected
                if (Mfeaturelayer.GetSelection().GetCount() < 1 || Mfeaturelayer.GetSelection().GetCount() > 1)
                {
                    FromMeasure = null;
                    ToMeasure = null;
                    return;
                }

                double minM, maxM;

                long SelectedOIDs = Mfeaturelayer.GetSelection().GetObjectIDs().FirstOrDefault();

                //Use inspector to get the shape
                var MinMaxInspector = new Inspector();

                MinMaxInspector.Load(Mfeaturelayer, SelectedOIDs);

                var MinMaxShape = MinMaxInspector["SHAPE"] as Polyline;

                //Main function for getting min max m values
                GeometryEngine.Instance.GetMinMaxM(MinMaxShape, out minM, out maxM);

                //set mvalue outputs to text boxes
                FromMeasure = Convert.ToString(minM);
                ToMeasure = Convert.ToString(maxM);

            });
        }

        private string _heading = "Set From/To Measures";
        public string _fromMeasure;
        public string _toMeasure;

        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        //From and To measure properties are bound to the corresponding .xaml text boxes
        public string FromMeasure
        {
            get { return _fromMeasure; }
            set
            {
                SetProperty(ref _fromMeasure, value, () => FromMeasure);

            }
        }

        public string ToMeasure
        {
            get { return _toMeasure; }
            set
            {
                SetProperty(ref _toMeasure, value, () => ToMeasure);
            }
        }

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SetFromTo_ShowButton : Button
        {
            protected override void OnClick()
            {
                SetFromToViewModel.Show();
            }
        }
    }
