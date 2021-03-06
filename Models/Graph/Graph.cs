﻿namespace Models
{
    using Factorial;
    using Models.Core;
    using Models.Storage;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Newtonsoft.Json;
    using Models.Core.Run;
    using Models.CLEM;
    using APSIM.Shared.Utilities;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a graph
    /// </summary>
    [ViewName("UserInterface.Views.GraphView")]
    [PresenterName("UserInterface.Presenters.GraphPresenter")]
    [Serializable]
    [ValidParent(ParentType = typeof(Simulations))]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(Zone))]
    [ValidParent(ParentType = typeof(Experiment))]
    [ValidParent(ParentType = typeof(Morris))]
    [ValidParent(ParentType = typeof(Sobol))]
    [ValidParent(ParentType = typeof(Folder))]
    [ValidParent(ParentType = typeof(GraphPanel))]
    public class Graph : Model, AutoDocumentation.ITag, ICustomDocumentation
    {
        /// <summary>The data tables on the graph.</summary>
        [NonSerialized]
        private Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        /// <summary>
        /// An enumeration for the position of the legend
        /// </summary>
        /// <remarks>
        /// fixme - we should support all valid OxyPlot legend position types.
        /// </remarks>
        public enum LegendPositionType
        {

            /// <summary>
            /// Place the legend box in the top-left corner.
            /// </summary>
            TopLeft = 0,

            /// <summary>
            ///     Place the legend box centered at the top.
            /// </summary>
            TopCenter = 1,

            /// <summary>
            /// Place the legend box in the top-right corner.
            /// </summary>
            TopRight = 2,

            /// <summary>
            /// Place the legend box in the bottom-left corner.
            /// </summary>
            BottomLeft = 3,

            /// <summary>
            /// Place the legend box centered at the bottom.
            /// </summary>
            BottomCenter = 4,

            /// <summary>
            /// Place the legend box in the bottom-right corner.
            /// </summary>
            BottomRight = 5,

            /// <summary>
            /// Place the legend box in the left-top corner.
            /// </summary>
            LeftTop = 6,

            /// <summary>
            /// Place the legend box centered at the left.
            /// </summary>
            LeftMiddle = 7,

            /// <summary>
            /// Place the legend box in the left-bottom corner.
            /// </summary>
            LeftBottom = 8,

            /// <summary>
            /// Place the legend box in the right-top corner.
            /// </summary>
            RightTop = 9,

            /// <summary>
            /// Place the legend box centered at the right.
            /// </summary>
            RightMiddle = 10,

            /// <summary>
            /// Place the legend box in the right-bottom corner.
            /// </summary>
            RightBottom = 11
        }

        /// <summary>
        /// An enumeration for the position of the stats / equation annotation.
        /// </summary>
        public enum AnnotationPositionType
        {

            /// <summary>
            /// Place the annotation box in the top-left corner.
            /// </summary>
            TopLeft = 0,

            /// <summary>
            ///     Place the annotation box in the top-right corner.
            /// </summary>
            TopRight = 1,

            /// <summary>
            /// Place the annotation box in the bottom-left corner.
            /// </summary>
            BottomLeft = 2,

            /// <summary>
            /// Place the annotation box in the bottom-right corner.
            /// </summary>
            BottomRight = 3,
        }

        /// <summary>
        /// An enumeration for the orientation of the legend items.
        /// </summary>
        public enum LegendOrientationType
        {
            /// <summary>
            /// Stack legend items vertically.
            /// </summary>
            Vertical,

            /// <summary>
            /// Stack legend items horizontally.
            /// </summary>
            Horizontal
        }

        /// <summary>
        /// Gets or sets the caption at the bottom of the graph
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets a list of all axes
        /// </summary>
        public List<Axis> Axis { get; set; }

        /// <summary>
        /// Gets or sets a list of all series
        /// </summary>
        [JsonIgnore]
        public List<Series> Series { get { return FindAllChildren<Series>().ToList(); } }

        /// <summary>
        /// Gets or sets the location of the legend
        /// </summary>
        public LegendPositionType LegendPosition { get; set; }

        /// <summary>
        /// Controls the orientation of legend items.
        /// </summary>
        public LegendOrientationType LegendOrientation { get; set; }

        /// <summary>
        /// Gets or sets the location of the annotations - name/position map.
        /// </summary>
        public AnnotationPositionType AnnotationPosition { get; set; }

        /// <summary>
        /// Gets or sets a list of raw grpah series that should be disabled.
        /// </summary>
        public List<string> DisabledSeries { get; set; }

        /// <summary>
        /// If set to true, the legend will be shown outside the graph area.
        /// </summary>
        public bool LegendOutsideGraph { get; set; }

        /// <summary>
        /// Descriptions of simulations that are in scope.
        /// </summary>
        [JsonIgnore]
        public List<SimulationDescription> SimulationDescriptions { get; set; }

        /// <summary>Gets the definitions to graph.</summary>
        /// <returns>A list of series definitions.</returns>
        /// <param name="storage">Storage service</param>
        /// <param name="simulationFilter">(Optional) Simulation name filter.</param>
        public IEnumerable<SeriesDefinition> GetDefinitionsToGraph(IStorageReader storage, List<string> simulationFilter = null)
        {
            EnsureAllAxesExist();

            var series = FindAllChildren<Series>().Where(g => g.Enabled);
            var definitions = new List<SeriesDefinition>();
            foreach (var s in series)
            {
                var seriesDefinitions = s.CreateSeriesDefinitions(storage, SimulationDescriptions, simulationFilter);
                definitions.AddRange(seriesDefinitions);
            }

            return definitions;
        }

        /// <summary>Gets the annotations to graph.</summary>
        /// <returns>A list of series annotations.</returns>
        public IEnumerable<IAnnotation> GetAnnotationsToGraph()
        {
            return FindAllChildren<IGraphable>()
                        .Where(g => g.Enabled)
                        .SelectMany(g => g.GetAnnotations());
        }

        /// <summary>
        /// Writes documentation for this function by adding to the list of documentation tags.
        /// </summary>
        /// <param name="tags">The list of tags to add to.</param>
        /// <param name="headingLevel">The level (e.g. H2) of the headings.</param>
        /// <param name="indent">The level of indentation 1, 2, 3 etc.</param>
        public void Document(List<AutoDocumentation.ITag> tags, int headingLevel, int indent)
        {
            if (IncludeInDocumentation)
                tags.Add(this);
        }

        /// <summary>
        /// Ensure that we have all necessary axis objects.
        /// </summary>
        private void EnsureAllAxesExist()
        {
            // Get a list of all axis types that are referenced by the series.
            List<Models.Axis.AxisType> allAxisTypes = new List<Models.Axis.AxisType>();
            foreach (Series series in Series)
            {
                allAxisTypes.Add(series.XAxis);
                allAxisTypes.Add(series.YAxis);
            }

            // Go through all graph axis objects. For each, check to see if it is still needed and
            // if so copy to our list.
            if (Axis == null)
                Axis = new List<Axis>();
            List<Axis> allAxes = new List<Axis>();
            bool unNeededAxisFound = false;
            foreach (Axis axis in Axis)
            {
                if (allAxisTypes.Contains(axis.Type))
                    allAxes.Add(axis);
                else
                    unNeededAxisFound = true;
            }

            // Go through all series and make sure an axis object is present in our AllAxes list. If
            // not then go create an axis object.
            bool axisWasAdded = false;
            foreach (Series S in Series)
            {
                Axis foundAxis = allAxes.Find(a => a.Type == S.XAxis);
                if (foundAxis == null)
                {
                    allAxes.Add(new Axis() { Type = S.XAxis });
                    axisWasAdded = true;
                }

                foundAxis = allAxes.Find(a => a.Type == S.YAxis);
                if (foundAxis == null)
                {
                    allAxes.Add(new Axis() { Type = S.YAxis });
                    axisWasAdded = true;
                }
            }

            if (unNeededAxisFound || axisWasAdded)
                Axis = allAxes;
        }
    }
}
