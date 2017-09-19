using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WowNet.Client;
using WowNet.Client.AI;
using WowNet.Client.Enums;
using WowNet.Client.Enums.Unit;
using WowNet.Client.Objects;
using WowNet.Common.Extensions;
using WowNet.Common.Tools;
using static WowNet.Client.Structs.Spells.Druid;


namespace MonkHeal
{
    
    [CombatRoutineMetaData(Class.Druid, Specialization.DruidRestoration, "Iker", "0.1", "DruidMaster",
        "DruidMaster")]
    public class DruidMaster : CombatRoutine
    {
        public DruidMaster()
        {
            // subscribe to these events to provide a menu/help menu to users if you like.
            MenuClicked += (sender, args) => MessageBox.Show($@"{sender?.ToString()} menu button was clicked.");
            HelpButtonClicked += (sender, args) => MessageBox.Show($@"{sender?.ToString()} help button was clicked.");

            
            Game.Instance.Disposing +=
                    (sender, args) => FileLogger.Instance.WriteLine("shutting down routine: " + MetaData.Name);

            // this determines how often your routine is checked  if it needs to tick or not.  
            // in this case CombatNeeded would only be checked at most every 5 seconds -- 
            // and subsequently your CombatAsync() called.
            Interval = 300.Milliseconds();
        }

        // if true, your CombatAsync() method is called in the games tick.
        // same goes for RestNeeded and PrepareNeeded
        public override bool CombatNeeded => Game.Instance.Player.HasAura(5487) ==false&& Game.Instance.Player.HasAura(783)==false&& Game.Instance.Player.HasAura(768) == false;
        public override bool RestNeeded { get; } = false;
        public override bool PrepareNeeded { get; } = false;

        public static WowLocalPlayer Me => Game.Instance.Player;

        //创建targetmanager目标选择器
        MyTargetManager targetmanager= new MyTargetManager();
        

        // you can override enable/disable/initialize
        // but its suggested you call the base method first 
        // before your stuff in the override.
        // e.g:

        public override void Initialize()
        {
            base.Initialize();
            Log.Debug("Init complete");
        }

        public override void Enable()
        {
            base.Enable();
            Log.Debug("Enable complete");
        }

        public override void Disable()
        {
            base.Disable();
            Log.Debug("Disable complete");
        }

        // called when CombatNeeded is true.dd
        public override async Task<bool> CombatAsync()
        {

            

            targetmanager.UpdateEnvironment();

            
            var a=targetmanager.Solution_BattlePreparing_withpeoplehirt();

            //if (targetmanager.Target == null)
            //{ Log.Debug("no target"); }
            //else
            //{
            if (targetmanager.Target != null)
            {
                Log.Debug(targetmanager.lowhealthfriendCOunt.ToString());
            }
            //}

            if (targetmanager.Target == null)
            { return true; }

            UseAbilities(a);
            return true;

            
            //如果所有队友都满血，铺回春 




       
            
            
          
            //UseAbilities(AbilityType.Damage);

            // if you want to delay do not use sleep, use task.delay 
            //Log.Information("Waiting 1 second");

            //await Task.Delay(1.Seconds());

            //Log.Information("Done waiting.");

            // you can also use spells directly.
            //var smite = Game.Instance.Spells.SpellBook[585];
            //var targetUnit = Me.GetTargetUnit();
            //var castSmite = targetUnit.IsValid && targetUnit.Distance <= smite.MaximumRange && targetUnit.IsValid;

            //if (castSmite)
            //{
            //    if (!Me.IsFacing(targetUnit))
            //    {
            //        Me.LookAt(targetUnit);
            //    }

            //    smite.Cast(targetUnit);
            //}

           
        }

       // public WowUnit GetHurtFriend(List<WowObject>)
        
        // called when Prepare needed is true.
        public override async Task<bool> PrepareAsync()
        {
            Log.Information("Waiting 1 second");
            await Task.Delay(1.Seconds());
            Log.Information("Done waiting.");
            return true;
        }

        // called when RestNeeded is true.
        public override async Task<bool> RestAsync()
        {
            Log.Information("Waiting 1 second");
            await Task.Delay(1.Seconds());
            Log.Information("Done waiting.");
            return true;
        }

        // events subscribed to by default
        protected override void OnCombatEnter()
        {
            Log.Information("Combat entered");
            base.OnCombatEnter();
        }

        protected override void OnCombatExit()
        {
            Log.Information("On combat exit");
            base.OnCombatExit();
        }

        protected override void OnDeath()
        {
            Log.Information("Local player died.");
            base.OnDeath();
        }

        //protected override void OnSpellSuccess(int spellID, string sourceGUID, string destinationGUID)
        //{
        //    Log.WriteLine(
        //        $"Spell success data: Id={spellID} sourceGUID={sourceGUID} destinationGUID={destinationGUID}");
        //    base.OnSpellSuccess(spellID, sourceGUID, destinationGUID);
        //}

        protected override void OnDispel(int spellID, string sourceGUID, string destinationGUID)
        {
            Log.WriteLine($"Dispel data: Id={spellID} sourceGUID={sourceGUID} destinationGUID={destinationGUID}");
            base.OnDispel(spellID, sourceGUID, destinationGUID);
        }

        protected override void SetUpAbilities()
        {
            WowAbility Rejuvenation = new WowAbility(Restoration.Rejuvenation ,0, () => (Game.Instance.IsInGame&&targetmanager.RejuventurationCheck),
                () => Restoration.Rejuvenation.IsUsable, targetmanager.Gettarget)
            {
                FacingRequired = false,
                IsTerrainClick = false,
                CancelCast = false,
                CancelChannel=false,
            };
            WowAbility HealingTouch = new WowAbility(Restoration.HealingTouch, 0, () => (Game.Instance.IsInGame && targetmanager.HealingTouchCheck),
                () => Restoration.HealingTouch.IsUsable, targetmanager.Gettarget)
            {
                FacingRequired = false,
                IsTerrainClick = false,
                CancelCast = false,
                CancelChannel = false,
            };
            WowAbility Regrowth = new WowAbility(Restoration.Regrowth, 0, () => (Game.Instance.IsInGame && targetmanager.Regrowth_use_check),
                () => Restoration.Regrowth.IsUsable, targetmanager.Gettarget)
            {
                FacingRequired = false,
                IsTerrainClick = false,
                CancelCast = false,
                CancelChannel = false,
            };

            WowAbility Lifebloom = new WowAbility(Restoration.Lifebloom, 0, () => (Game.Instance.IsInGame && targetmanager.CanUse_Lifebloom),
                () => Restoration.Lifebloom.IsUsable, targetmanager.Gettarget)
            {
                FacingRequired = false,
                IsTerrainClick = false,
                CancelCast = false,
                CancelChannel = false,
            };

            WowAbility Wildgrowth = new WowAbility(Restoration.WildGrowth, 0, () => (Game.Instance.IsInGame && targetmanager.WildGrouth_use_check),
                () => Restoration.WildGrowth.IsUsable, targetmanager.Gettarget)
            {
                FacingRequired = false,
                IsTerrainClick = false,
                CancelCast = false,
                CancelChannel = false,
            };

            //BattlePreparing Spell list
            targetmanager.BattlePreparing.Add(Rejuvenation);

            //LeveL1_Healing Spell list
            targetmanager.LeveL1_Healting.Add(Wildgrowth);
            targetmanager.LeveL1_Healting.Add(Lifebloom);
            targetmanager.LeveL1_Healting.Add(Rejuvenation);
            targetmanager.LeveL1_Healting.Add(HealingTouch);
            targetmanager.LeveL1_Healting.Add(Regrowth);

            base.SetUpAbilities();
           
        }

        protected override bool HandleTaskException(AggregateException exception)
        {
            // any exception thrown by CombatAsync(), PrepareAsync(),
            // or RestAsync() is passed to this. If you call
            // base.HandleTaskException(exception); it will be logged and thrown.

            // example of handling an exception you throw in CombatAsync

            if (exception == null)
            {
                return true;
            }

            Log.WriteLine("Exception message " + exception.Message);

            return false;
        }
    }
}
