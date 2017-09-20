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
using WowNet.Common.AI;
using static WowNet.Client.Structs.Spells.Monk;

namespace MonkHeal
{
    public class MyTargetManager
    {
        //各类设置
        //丰饶预铺层数
        public int FengRao = 4;
        public int health_lifebloomCheck= 100;
        public int Regrowth_healthcheck = 90;
        public int HealingTouch_HeallthCheck = 70;
        public int SwiftmendHealtheCheck = 40;
        


        //各类单位集合
        public static List<WowPlayer> objects;
        public static List<WowPlayer> players;
        public static List<WowPlayer> friends;
        public static List<WowPlayer> friendsAndme;
        public static List<WowPlayer> enemies;

        //各类单位目标
        public static WowLocalPlayer Me;
        public WowPlayer lowestHealthFriend;
        public WowPlayer Target;
        public WowUnit MycurrentTarget;
        public int lowhealthfriendCOunt;

        
        public List<IAbility> BattlePreparing=new List<IAbility>();
        public List<IAbility> LeveL1_Healting = new List<IAbility>();
        public List<IAbility> HearTheLowestTarget = new List<IAbility>();

        public bool CancelChannel = true;
        public bool Rejuvenation_use_check = true;
        public bool Healingtouch_use_check = true;
        public bool Lifebloom_use_check = true;
        public bool Regrowth_use_check = true;
        public bool WildGrouth_use_check = true;
        public bool SwiftMent_use_check = true;

        public int RejuventurationCount = 0;
        //目标排序方法（按照血量）
        public static int CompareByHeathPercentage(WowPlayer player1, WowPlayer player2)
        {
            if (player1 == null || player2 == null)
                return 0;

            if (player1.HealthPercentage==player2.HealthPercentage)
            {
                return 0;
            }
            else
            {
                if (player1.HealthPercentage > player2.HealthPercentage)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }


        }
        

        public MyTargetManager()
        {
            
        }


        //获得用于战斗逻辑的各项战斗条件
        public void UpdateEnvironment()
        {
             

        //各类单位集合更新数据 
            Me = Game.Instance.Player;
            players = Game.Instance.Manager.Objects.OfType<WowPlayer>().Where(x=>x.Distance<40&&x.InLoS).ToList();
            foreach(var player in players)
            {
                player.Update();
            }
            friends = players.Where(x => x.IsFriendly).ToList();
            friendsAndme = friends;
            friendsAndme.Add(Me);
            friendsAndme.Sort(CompareByHeathPercentage);
            //血量最低队友
            lowestHealthFriend = friendsAndme.First();
            //更新血量低于70%的队友数量
            lowhealthfriendCOunt = friendsAndme.Where(x => x.HealthPercentage < 70).Count();
            enemies = players.Where(x => x.IsHostile).ToList();
            MycurrentTarget = null;
            MycurrentTarget = Game.Instance.Player.GetTargetUnit();

            //丰饶层数更新
            MycurrentTarget = Me.GetTargetUnit();
            
            RejuventurationCount = Me.GetAura(207640).StackCount;
            

            //更新生命绽放使用条件
            Lifebloom_use_check = UpdateCanuse_Lifebloom();
            Rejuvenation_use_check = true;
            Healingtouch_use_check = true;
            Regrowth_use_check = UpdateRegrowthhealth();
            WildGrouth_use_check = UpdateWildGrowthCheck();
            Target = null;
        }


        //对是否使用生命绽放进行检测
        

        public bool RejuvenationCheck_checkthetarget(WowUnit target)
        {
            if (target.HealthPercentage > 80)
            {
                return !target.HasAura(774);

            }
            else
            {
                return !target.HasAura(774) || !target.HasAura(155777);
            }
        }
        public bool UpdateWildGrowthCheck()
        {
            if (lowhealthfriendCOunt >= 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool UpdateSwiftMend(WowUnit target)
        {
            if (target.HealthPercentage < SwiftmendHealtheCheck)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateCanuse_Lifebloom()
        {
            foreach(var player in friendsAndme)
            {
                player.Update();
                if (player.HasAura(33763) && player.HealthPercentage < health_lifebloomCheck)
                {
                    return false;

                }
                
            }
            return true;
        }
        public bool UpdateRegrowthhealth()
        {
            if (Me.HasAura(16870))
            {
                Regrowth_healthcheck = 90;
                return true;
            }
            else
            {
                Regrowth_healthcheck = 70;
                return true;
            }
        }
        
        

        public List<IAbility> Solution_BattlePreparing()
        {
            



            if (RejuventurationCount < FengRao)
            {
                if (RejuvenationCheck_checkthetarget(Me))
                {
                    Target = Me;
                }
                else if(MycurrentTarget!=null&&RejuvenationCheck_checkthetarget(MycurrentTarget)&&MycurrentTarget.IsAPlayer&&MycurrentTarget.IsFriendly){
                    Target = MycurrentTarget as WowPlayer;
                }
                else
                {
                    Target = friendsAndme.FirstOrDefault(x =>RejuvenationCheck_checkthetarget(x)==true);
                }
            }
            return BattlePreparing;
        }


        public List<IAbility> Solution_BattlePreparing_withpeoplehirt()
        {
            


            Target = friendsAndme.FirstOrDefault(x => x.HealthPercentage < 95 && (RejuvenationCheck_checkthetarget(x) == true || Lifebloom_use_check == true || x.HealthPercentage<Regrowth_healthcheck));
            if (Target != null)
            {
                if (Target.HealthPercentage < Regrowth_healthcheck)
                {
                    Regrowth_use_check = true;
                }
                else { Regrowth_use_check = false; }

                if (Target.HealthPercentage < HealingTouch_HeallthCheck && FengRao >= 7)
                {
                    Healingtouch_use_check = true;
                }
                else { Healingtouch_use_check = false; }

                return LeveL1_Healting;
            }
            else
            {
                return Solution_BattlePreparing();
            }
        }




        //public List<IAbility> Solution_Level1_allmorethan90()
        //{
        //    if (Lifebloom_use_check == true)
        //    {
        //        Target = lowestHealthFriend;
        //        return LeveL1_Healting;
        //    } else 
        //    {
        //        Target = friendsAndme.FirstOrDefault(x => (!x.HasAura(774) || !x.HasAura(155777))&&x.HealthPercentage<90);
        //        if (Target != null)
        //        {
        //            Rejuvenation_use_check = true;
        //            return LeveL1_Healting;
        //        }
        //        return Solution_BattlePreparing();
        //    }
            
            
        //}



        public WowUnit Gettarget()
        {
            //对目标进行回春术释放修正
            if (Target != null)
            {
                Rejuvenation_use_check = RejuvenationCheck_checkthetarget(Target);
                SwiftMent_use_check = UpdateSwiftMend(Target);
            }
            
            return Target;
        }
    }
}
