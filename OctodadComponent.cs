using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace LiveSplit.Octodad
{
    class OctodadComponent : IComponent
    {
        Process Octodad { get; set; }
        LiveSplitState State { get; set; }
        TimerModel Model { get; set; }
        String CurrentLevel { get; set; }

        public OctodadSettings Settings { get; set; }

        System.Timers.Timer PauseTimer { get; set; }
        TripleDateTime FirstLoadMessage { get; set; }
        TripleDateTime LastLoadMessage { get; set; }

        public string ComponentName
        {
            get { return "Octodad Auto Splits"; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        protected InfoTimeComponent InternalComponent { get; set; }

        public OctodadComponent()
        {
            Settings = new OctodadSettings();

            InternalComponent = new InfoTimeComponent(null, null, new RegularTimeFormatter(TimeAccuracy.Hundredths));

            ContextMenuControls = new Dictionary<String, Action>();
            ContextMenuControls.Add("Start Octodad", ConnectToOctodad);
            PauseTimer = new System.Timers.Timer(2000)
            {
                Enabled = false
            };
            PauseTimer.Elapsed += PauseTimer_Elapsed;
        }

        void PauseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PauseTimer.Stop();
            var loadTime = LastLoadMessage - FirstLoadMessage;
            if (State.CurrentPhase != TimerPhase.NotRunning || State.CurrentPhase != TimerPhase.Ended)
            {
                State.LoadingTimes += loadTime;
            }
            //Debug.WriteLine("Loading Times: " + new RegularTimeFormatter(TimeAccuracy.Hundredths).Format(loadTime) + "s");
        }

        public String GetSplitLevelName(int splitIndex)
        {
            switch (splitIndex)
            {
                case -1: return "Church_Main";
                case 0: return "OpeningCredits";
                case 1: return "Grocery_Main";
                case 2: return "Aquarium_Hub";
                case 3: return "Kelp_Main";
                case 4: return "Aquarium_Hub";
                case 5: return "Aquarium_Hub";
                case 6: return "Aquarium_Hub";
                case 7: return "Aquarium_Swimming_Main";
                case 8: return "Stealth_Main";
                case 9: return "Cafeteria_Main";
                case 10: return "EndingCredits";
            }
            return "";
        }

        public void ConnectToOctodad()
        {
            try
            {
                var psi = new ProcessStartInfo(Settings.Path)
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(Settings.Path),
                    //Arguments = "-FORCELOGFLUSH -LOG"
                };
                Octodad = new Process();
                Octodad.StartInfo = psi;
                Octodad.Start();

                new Thread(ProcessConsole).Start();
            }
            catch
            {
                MessageBox.Show("Octodad couldn't be started.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ProcessConsole()
        {
            while (true)
            {
                var line = Octodad.StandardOutput.ReadLine();
                if (line == null)
                    return;

                if (line.StartsWith("Loaded ") && PauseTimer.Enabled)
                {
                    LastLoadMessage = TripleDateTime.Now;
                    PauseTimer.Stop();
                    PauseTimer.Start();
                }

                if (line.StartsWith("~~~~~~~~~~~~~~~  LOADING LEVEL: Content/Levels/"))
                {
                    CurrentLevel = line.Substring(line.LastIndexOf("/") + 1);
                    CurrentLevel = CurrentLevel.Substring(0, CurrentLevel.LastIndexOf("."));

                    var splitLevelName = GetSplitLevelName(State.CurrentPhase == TimerPhase.NotRunning ? -1 : State.CurrentSplitIndex);

                    if (CurrentLevel == splitLevelName)
                    {
                        if (State.CurrentPhase == TimerPhase.NotRunning)
                            Model.Start();
                        else
                            Model.Split();
                    }

                    FirstLoadMessage = LastLoadMessage = TripleDateTime.Now;
                    PauseTimer.Start();

                    //Debug.WriteLine("Loading Level: " + CurrentLevel);
                }
                Debug.WriteLine(line);
            }
        }

        void State_OnStart(object sender, EventArgs e)
        {
        }

        public float VerticalHeight
        {
            get { return Settings.ShowActualTimeAsWell ? InternalComponent.VerticalHeight : 0; }
        }

        public float MinimumWidth
        {
            get { return Settings.ShowActualTimeAsWell ? InternalComponent.MinimumWidth : 0; }
        }

        public float HorizontalWidth
        {
            get { return Settings.ShowActualTimeAsWell ? InternalComponent.HorizontalWidth : 0; }
        }

        public float MinimumHeight
        {
            get { return Settings.ShowActualTimeAsWell ? InternalComponent.MinimumHeight : 0; }
        }

        public System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return Settings;
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            if (Settings.ShowActualTimeAsWell)
            {
                InternalComponent.DrawHorizontal(g, state, height, clipRegion);
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            if (Settings.ShowActualTimeAsWell)
            {
                InternalComponent.DrawVertical(g, state, width, clipRegion);
            }
        }

        public float PaddingBottom
        {
            get { return InternalComponent.PaddingBottom; }
        }

        public float PaddingLeft
        {
            get { return InternalComponent.PaddingLeft; }
        }

        public float PaddingRight
        {
            get { return InternalComponent.PaddingRight; }
        }

        public float PaddingTop
        {
            get { return InternalComponent.PaddingTop; }
        }

        public void RenameComparison(string oldName, string newName)
        {
        }

        public void Update(UI.IInvalidator invalidator, LiveSplitState state, float width, float height, UI.LayoutMode mode)
        {
            if (State != state)
            {
                State = state;
                if (!State.Run.CustomComparisons.Contains(LiveSplitState.GameTimeComparisonName))
                    State.Run.CustomComparisons.Add(LiveSplitState.GameTimeComparisonName);

                Model = new TimerModel() { CurrentState = State };
                State.OnStart += State_OnStart;
            }

            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.Text = "Time With Loads";
            InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.Font = InternalComponent.NameLabel.Font;

            var newValue = State.CurrentGameTime;

            if (invalidator != null && InternalComponent.TimeValue.ToString() != newValue.ToString())
            {
                invalidator.Invalidate(0, 0, width, height);
            }

            InternalComponent.TimeValue = newValue;
        }
    }
}
