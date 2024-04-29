using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace LandValueRemake
{
    [FileLocation(nameof(LandValueRemake))]
    [SettingsUIGroupOrder(kOptionsGroup, kButtonGroup)]
    [SettingsUIShowGroupName(kOptionsGroup, kButtonGroup)]

    public class Setting : ModSetting
    {
        public const string kSection = "Main";

        public const string kOptionsGroup = "Options";
        public const string kButtonGroup = "Actions";

        public Setting(IMod mod) : base(mod)
        {
            SetDefaults();
        }

        [SettingsUIHidden]
        public bool _Hidden { get; set; }

        [SettingsUISection(kSection, kOptionsGroup)]
        public bool Logging { get; set; }

        [SettingsUISection(kSection, kOptionsGroup)]
        public bool EnableRealisticLVSimulation { get; set; }

        [SettingsUISection(kSection, kOptionsGroup)]
        public bool EraseLowResHighRentWarning { get; set; }

        [SettingsUISection(kSection, kOptionsGroup)]
        public bool EnableLowResBuildEverywhere { get; set; }

        [SettingsUISection(kSection, kOptionsGroup)]
        public bool EnableLevelingCostAdjust { get; set; }

        public override void SetDefaults()
        {
            _Hidden = true;

            Logging = false;

            EnableRealisticLVSimulation = true;

            EnableLowResBuildEverywhere = true;

            EraseLowResHighRentWarning = true;

            EnableLevelingCostAdjust = false;

        }

    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
        {
            { m_Setting.GetSettingsLocaleID(),$"LandValueRemake {Mod.ModAsset.version}" },
            { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

            { m_Setting.GetOptionGroupLocaleID(Setting.kOptionsGroup), "Options" },
            { m_Setting.GetOptionGroupLocaleID(Setting.kButtonGroup), "Actions" },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Logging)), "Detailed logging" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.Logging)), "Outputs more diagnostics information to the log file." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableRealisticLVSimulation)), "Enable More Realistic LandValue Simulation" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableRealisticLVSimulation)), "Enable More Realistic LandValue Simulation base on original vanilla game,adjust LV by zoning type,and mixed new LV feature since v1.1.x ." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableLowResBuildEverywhere)), "Enable to Build LowRes almost everywhere" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableLowResBuildEverywhere)), "Enable to Build LowRes in high landvalue zone,and Lower the landvalue of its location(ignoring the red road and it'll turn green)." },                        

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EraseLowResHighRentWarning)), "Erase LowRes HighRent Warning" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EraseLowResHighRentWarning)), "Erase LowRes HighRent Warning." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableLevelingCostAdjust)), "Adjust building upgrade costs by zoning type(not available for now)" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableLevelingCostAdjust)), "Adjust building upgrade costs by zoning type(not available for now)." },


        };
        }



        public void Unload()
        {
        }

    }


    public class LocaleCN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleCN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(),$"LandValueRemake{Mod.ModAsset.version}" },
            { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

            { m_Setting.GetOptionGroupLocaleID(Setting.kOptionsGroup), "Options" },
            { m_Setting.GetOptionGroupLocaleID(Setting.kButtonGroup), "Actions" },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Logging)), "详细日志" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.Logging)), "输出详细诊断日志." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableRealisticLVSimulation)), "开启更加现实的地价系统模拟" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableRealisticLVSimulation)), "开启更加现实的地价系统模拟(基于旧版地价系统并修复bug，增加按住宅、商业、工业、办公分区类型分别计算地价功能，并整合新版地价特色)." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableLowResBuildEverywhere)), "允许任意地点建造低密住宅" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableLowResBuildEverywhere)), "允许低密度住宅可在高地价土地上建造(请无视红色路段在几乎任何地方可建)，并降低所在路段地价(变绿)." },                        

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EraseLowResHighRentWarning)), "去除低密住宅高租金警告" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EraseLowResHighRentWarning)), "一键强制去除低密住宅高租金警告." },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableLevelingCostAdjust)), "按分区类型调整建筑升级花费(暂不可用)" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableLevelingCostAdjust)), "按分区类型调整建筑升级花费(暂不可用)." },

            };
        }
        public void Unload()
        {
        }

    }
}
