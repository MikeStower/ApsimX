﻿using Models.Core;
using Models.CLEM.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Models.CLEM.Resources;

namespace Models.CLEM.Groupings
{
    ///<summary>
    /// Contains a group of filters to identify individual ruminants
    ///</summary> 
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(RuminantActivityFeed))]
    [Description("This ruminant filter group selects specific individuals from the ruminant herd using any number of Ruminant Filters. This filter group includes feeding rules. No filters will apply rules to current herd. Multiple feeding groups will select groups of individuals required.")]
    [Version(1, 0, 1, "")]
    [HelpUri(@"Content/Features/Filters/RuminantFeedGroup.htm")]
    public class RuminantFeedGroup: CLEMModel, IFilterGroup
    {
        /// <summary>
        /// Value to supply for each month
        /// </summary>
        [Description("Value to supply")]
        [GreaterThanValue(0)]
        public double Value { get; set; }

        /// <summary>
        /// Combined ML ruleset for LINQ expression tree
        /// </summary>
        [XmlIgnore]
        public object CombinedRules { get; set; } = null;

        /// <summary>
        /// Proportion of group to use
        /// </summary>
        [XmlIgnore]
        public double Proportion { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RuminantFeedGroup()
        {
            base.ModelSummaryStyle = HTMLSummaryStyle.SubActivity;
        }

        /// <summary>
        /// Provides the description of the model settings for summary (GetFullSummary)
        /// </summary>
        /// <param name="formatForParentControl">Use full verbose description</param>
        /// <returns></returns>
        public override string ModelSummary(bool formatForParentControl)
        {
            string html = "";

            if(this.Parent.GetType() != typeof(RuminantActivityFeed))
            {
                html += "<div class=\"warningbanner\">This Ruminant Feed Group must be placed beneath a Ruminant Activity Feed component</div>";
                return html;
            }

            RuminantFeedActivityTypes ft = (this.Parent as RuminantActivityFeed).FeedStyle;
            html += "\n<div class=\"activityentry\">";
            switch (ft)
            {
                case RuminantFeedActivityTypes.SpecifiedDailyAmount:
                case RuminantFeedActivityTypes.SpecifiedDailyAmountPerIndividual:
                    html += "<span class=\"" + ((Value <= 0) ? "errorlink" : "setvalue") + "\">"+Value.ToString() + "kg</span>";
                    break;
                case RuminantFeedActivityTypes.ProportionOfFeedAvailable:
                case RuminantFeedActivityTypes.ProportionOfWeight:
                case RuminantFeedActivityTypes.ProportionOfPotentialIntake:
                case RuminantFeedActivityTypes.ProportionOfRemainingIntakeRequired:
                    if (Value != 1)
                    {
                        html += "<span class=\"" + ((Value <= 0) ? "errorlink" : "setvalue") + "\">"+Value.ToString("0.##%")+"</span>";
                    }
                    break;
                default:
                    break;
            }

            string starter = " of ";
            if (Value == 1)
            {
                starter = "The ";
            }

            html += "<span class=\"setvalue\">";
            switch (ft)
            {
                case RuminantFeedActivityTypes.ProportionOfFeedAvailable:
                    html += " of the available food supply";
                    break;
                case RuminantFeedActivityTypes.SpecifiedDailyAmountPerIndividual:
                    html += " per individual per day";
                    break;
                case RuminantFeedActivityTypes.SpecifiedDailyAmount:
                    html += " per day";
                    break;
                case RuminantFeedActivityTypes.ProportionOfWeight:
                    html += starter + "live weight";
                    break;
                case RuminantFeedActivityTypes.ProportionOfPotentialIntake:
                    html += starter + "potential intake";
                    break;
                case RuminantFeedActivityTypes.ProportionOfRemainingIntakeRequired:
                    html += starter + "remaining intake";
                    break;
                default:
                    break;
            }
            html += "</span> ";
            switch (ft)
            {
                case RuminantFeedActivityTypes.ProportionOfFeedAvailable:
                    html += "will be fed to all individuals that match the following conditions:";
                    break;
                case RuminantFeedActivityTypes.SpecifiedDailyAmount:
                    html += "combined is fed to all individuals that match the following conditions:";
                    break;
                case RuminantFeedActivityTypes.SpecifiedDailyAmountPerIndividual:
                    html += "is fed to each individual that matches the following conditions:";
                    break;
                default:
                    html += "is fed to the individuals that match the following conditions:";
                    break;
            }
            html += "</div>";

            html += "\n<div class=\"activityentry\">";
            html += "Individual's intake will automatically be limited to 1.2 x potential intake, with excess food still utilised";
            html += "</div>";

            if (ft == RuminantFeedActivityTypes.SpecifiedDailyAmount)
            {
                html += "<div class=\"warningbanner\">Note: This is a specified daily amount fed to the entire herd. If insufficient provided, each individual's potential intake will not be met</div>";
            }

            return html;
        }

        /// <summary>
        /// Provides the closing html tags for object
        /// </summary>
        /// <returns></returns>
        public override string ModelSummaryInnerClosingTags(bool formatForParentControl)
        {
            string html = "";
            html += "\n</div>";
            return html;
        }

        /// <summary>
        /// Provides the closing html tags for object
        /// </summary>
        /// <returns></returns>
        public override string ModelSummaryInnerOpeningTags(bool formatForParentControl)
        {
            string html = "";
            html += "\n<div class=\"filterborder clearfix\">";
            if (!(this.FindAllChildren<RuminantFilter>().Count() >= 1))
            {
                html += "<div class=\"filter\">All individuals</div>";
            }
            return html;
        }

    }
}
