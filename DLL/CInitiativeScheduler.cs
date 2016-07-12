using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace TMonitor
{
    public delegate void InitiativeAction();
    public class CInitiative
    {
        InitiativeAction action;
        public double timeDelay;//В миллисекундах
        public bool repeatMode;
        public string title { get { return _title; } }
        public string description { get { return _description; } }

        string _title;
        string _description;
        Timer timer;
        
        public CInitiative(InitiativeAction pAction, double pTimeDelay, bool pRepeatMode = false)
        {
            action = pAction;
            timer = new Timer(pTimeDelay);
            timer.AutoReset = pRepeatMode;
            timer.Elapsed += _OnTimerElapsed;
            timer.Start();
        }
        public CInitiative(InitiativeAction pAction, double pTimeDelay, bool pRepeatMode = false, bool pStartRightNow = true)
        {
            action = pAction;
            timer = new Timer(pTimeDelay);
            timer.AutoReset = pRepeatMode;
            timer.Elapsed += _OnTimerElapsed;
            if(pStartRightNow)
                timer.Start();
        }
        public CInitiative(InitiativeAction pAction, double pTimeDelay, bool pRepeatMode = false, bool pStartRightNow = true, string pTitle = "", string pDescription = "")
        {
            action = pAction;
            timer = new Timer(pTimeDelay);
            timer.AutoReset = pRepeatMode;
            timer.Elapsed += _OnTimerElapsed;
            _title = pTitle;
            _description = pDescription;
            if (pStartRightNow)
                timer.Start();
        }
        public CInitiative(InitiativeAction pAction, uint pTimeDelay, bool pRepeatMode, InitiativeAction pOnSuccessAction, InitiativeAction pOnFailureAction)
        {
            action = pAction;
            timer = new Timer(pTimeDelay);
            timer.AutoReset = pRepeatMode;
            timer.Elapsed += _OnTimerElapsed;
        }
        ~CInitiative()
        {
            timer.Stop();
            timer.Close();
            timer.Dispose();
            timer = null;
        }
        public void Destroy()
        {
            timer.Stop();
            timer.Close();
            timer.Dispose();
            timer = null;
        }
        //Methods
        public void Start()
        {
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }
        public void ChangeTimeDelay(double pTimeDelay)
        {
            timeDelay = pTimeDelay;
        }
        //Event delegates
        InitiativeAction OnBeforeExecuteInitiative;
        InitiativeAction OnSuccessInitiative;
        InitiativeAction OnFailureInitiative;
        void _OnSuccessInitiative()
        {
            if (OnSuccessInitiative != null)
                OnSuccessInitiative();
        }
        void _OnFailureInitiative()
        {
            if (OnFailureInitiative != null)
                OnFailureInitiative();
        }
        void _OnBeforeExecuteInitiative()
        {
            if (OnBeforeExecuteInitiative != null)
                OnBeforeExecuteInitiative();
        }
        void _OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _OnBeforeExecuteInitiative();
                action();
                _OnSuccessInitiative();
            }
            catch
            {
                _OnFailureInitiative();
            }
        }
    }
    
    public abstract class CInitiativeService
    {
        private Dictionary<InitiativeAction, Timer> schedule;
        public abstract void AddInitiative(CInitiative pInitiative, uint pTimeDelay, bool pRepeatMode = false);
        public abstract void DeleteInitiative(InitiativeAction pAction);
    }
    class CInitiativeScheduler
    {
    }
}
