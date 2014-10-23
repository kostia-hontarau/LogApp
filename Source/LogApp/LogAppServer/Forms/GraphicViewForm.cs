using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using LogApp.Common.Model;

namespace LogAppServer.Forms
{
    public partial class GraphicViewForm : Form
    {
        #region Properties
        public ApplicationInfoCollection State { get; set; } 
        #endregion

        #region Constructors
        public GraphicViewForm()
        {
            InitializeComponent();
        } 
        #endregion

        #region Event Handlers
        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void GraphicViewForm_Shown(object sender, EventArgs e)
        {
            this.LoadPieChart();
        } 
        #endregion

        #region Assistants
        private void LoadPieChart()
        {
            if (this.State == null)
            {
                return;
            }
            pieChart.Titles.Clear();
            pieChart.Titles.Add("Активность процесса за время мониторинга, %");
            Series series = this.pieChart.Series["processActivities"];
            series.Points.Clear();

            TimeSpan totalTime = new TimeSpan();
            totalTime = this.State
                .Where(info => info.ActivityTime.HasValue)
                .Aggregate(totalTime, (current, info) => current.Add(info.ActivityTime.Value));

            List<ApplicationInfo> listPoints = this.State.ToList();
            listPoints.Sort(new ApplicationInfoActivityComparer());
            var moreActive = listPoints
                .SkipWhile(item => item.ActivityTime == null)
                .Take(5);
            double counted = 0;
            string withFormat = null;
            foreach (ApplicationInfo info in moreActive)
            {
                DataPoint point = new DataPoint();
                double percent = ((info.ActivityTime.Value.TotalSeconds) / (totalTime.TotalSeconds)) * 100;
                withFormat = percent.ToString("###.00");
                double result = double.Parse(withFormat);
                counted += result;
                point.SetValueXY(info.ToString(), result);
                series.Points.Add(point);
            }

            DataPoint others = new DataPoint();
            withFormat = (100 - counted).ToString("###.00");

            others.SetValueXY("Другие", double.Parse(withFormat));
            series.Points.Add(others);

            pieChart.Invalidate();
        } 
        #endregion
    }

    internal class ApplicationInfoActivityComparer : IComparer<ApplicationInfo>
    {
        public int Compare(ApplicationInfo x, ApplicationInfo y)
        {
            if (!x.ActivityTime.HasValue && !y.ActivityTime.HasValue) return 0;
            if (!x.ActivityTime.HasValue) return -1;
            if (!y.ActivityTime.HasValue) return 1;
            
            return x.ActivityTime.Value > y.ActivityTime.Value ?
                                -1 :
                                (x.ActivityTime.Value == y.ActivityTime.Value) ? 0 : 1;
        }
    }
}
