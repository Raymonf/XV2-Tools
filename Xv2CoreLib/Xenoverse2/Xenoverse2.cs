﻿using System;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xv2CoreLib.CMS;
using Xv2CoreLib.CUS;
using Xv2CoreLib.MSG;
using Xv2CoreLib.BCS;
using Xv2CoreLib.BAC;
using Xv2CoreLib.BDM;
using Xv2CoreLib.BSA;
using Xv2CoreLib.CSO;
using Xv2CoreLib.EAN;
using Xv2CoreLib.ERS;
using Xv2CoreLib.IDB;
using Xv2CoreLib.PUP;
using Xv2CoreLib.BCM;
using Xv2CoreLib.ACB;
using Xv2CoreLib.BAI;
using Xv2CoreLib.AMK;
using Xv2CoreLib.BAS;
using Xv2CoreLib.ESK;
using Xv2CoreLib.EMD;
using Xv2CoreLib.PSC;
using Xv2CoreLib.EMM;
using Xv2CoreLib.EMB_CLASS;
using Xv2CoreLib.EffectContainer;
using Xv2CoreLib.Resource;
using static Xv2CoreLib.CUS.CUS_File;
using System.Threading.Tasks;

namespace Xv2CoreLib
{
    public sealed class Xenoverse2
    {
        public enum Language
        {
            English, 
            Spanish1,
            Spanish2,
            French,
            German,
            Italian,
            Portuguese,
            Polish,
            Russian,
            Chinese1,
            Chinese2,
            Korean,

            NumLanguages
        }

        public enum CustomCharacter
        {
            HUM = 100,
            HUF = 101,
            SYM = 102,
            SYF = 103,
            NMC = 104,
            FRI = 105,
            MAM = 106,
            MAF = 107,
            MAP = 108
        }

        public enum MoveFileTypes
        {
            BAC,
            BCM,
            BSA,
            BDM,
            SHOT_BDM,
            EEPK,
            SE_ACB,
            VOX_ACB,
            EAN,
            CAM_EAN,
            BAS,
            AFTER_BAC,
            AFTER_BCM,
            BAI,
            AMK,
            FCE_EAN,
            TAL_EAN //CMN
        }

        public enum MoveType
        {
            Skill,
            Character,
            Common
        }

        public static readonly string[] LanguageSuffix = new string[12] { "en.msg", "es.msg", "ca.msg", "fr.msg", "de.msg", "it.msg", "pt.msg", "pl.msg", "ru.msg", "tw.msg", "zh.msg", "kr.msg" };
        public static readonly string[] LanguageSuffixNoExt = new string[12] { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr" };

        //Singleton
        private static Lazy<Xenoverse2> instance = new Lazy<Xenoverse2>(() => new Xenoverse2());
        public static Xenoverse2 Instance => instance.Value;

        //File Paths
        public const string ERS_PATH = "vfx/vfx_spec.ers";
        public const string CUS_PATH = "system/custom_skill.cus";
        public const string CMS_PATH = "system/char_model_spec.cms";
        public const string PUP_PATH = "system/powerup_parameter.pup";
        public const string CSO_PATH = "system/chara_sound.cso";
        public const string PSC_PATH = "system/parameter_spec_char.psc";
        public const string SKILL_IDB_PATH = "system/item/skill_item.idb";
        public const string TALISMAN_IDB_PATH = "system/item/talisman_item.idb";
        public const string CMN_BAC_PATH = "chara/CMN/CMN.bac";
        public const string CMN_EAN_PATH = "chara/CMN/CMN.ean";
        public const string CMN_CAM_EAN_PATH = "chara/CMN/CMN.cam.ean";
        public const string CMN_BDM_PATH = "chara/CMN/CMN.bdm";
        public const string CMN_BSA_PATH = "skill/CMN/CMN.bsa";
        public const string CMN_SHOT_BDM_PATH = "skill/CMN/CMN_PLAYER.shot.bdm";
        public const string CMN_SE_ACB_PATH = "sound/SE/Battle/Common/CAR_BTL_CMN.acb";
        public const string CMN_EEPK_PATH = "vfx/cmn/BTL_CMN.eepk";
        public const string CMN_TAL_EAN = "chara/CMN/CMN.tal.ean";

        public const string CHARACTER_MSG_PATH = "msg/proper_noun_character_name_";
        public const string SUPER_SKILL_MSG_PATH = "msg/proper_noun_skill_spa_name_";
        public const string ULT_SKILL_MSG_PATH = "msg/proper_noun_skill_ult_name_";
        public const string AWOKEN_SKILL_MSG_PATH = "msg/proper_noun_skill_met_name_";
        public const string EVASIVE_SKILL_MSG_PATH = "msg/proper_noun_skill_esc_name_";
        public const string SUPER_SKILL_DESC_MSG_PATH = "msg/proper_noun_skill_spa_info_";
        public const string ULT_SKILL_DESC_MSG_PATH = "msg/proper_noun_skill_ult_info_";
        public const string AWOKEN_SKILL_DESC_MSG_PATH = "msg/proper_noun_skill_met_info_";
        public const string EVASIVE_SKILL_DESC_MSG_PATH = "msg/proper_noun_skill_esc_info_";
        public const string BTLHUD_MSG_PATH = "msg/quest_btlhud_";

        //Load bools
        public bool loadCmn = false;
        public bool loadSkills = true;
        public bool loadCharacters = true;

        //System Files
        private CUS_File cusFile = null;
        private CMS_File cmsFile = null;
        private ERS_File ersFile = null;
        private IDB_File skillIdbFile = null;
        private PUP_File pupFile = null;
        private CSO_File csoFile = null;
        private PSC_File pscFile = null;

        //Cmn Files
        public EAN_File CmnEan = null;
        public EAN_File CmnCamEan = null;
        public BAC_File CmnBac = null;
        public BDM_File CmnBdm = null;

        //Msg Files
        private MSG_File[] charaNameMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] spaSkillNameMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] ultSkillNameMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] evaSkillNameMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] metSkillNameMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] spaSkillDescMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] ultSkillDescMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] evaSkillDescMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] metSkillDescMsgFile = new MSG_File[(int)Language.NumLanguages];
        private MSG_File[] btlHudMsgFile = new MSG_File[(int)Language.NumLanguages];

        //Misc variables
        private FileWatcher fileWatcher => FileManager.Instance.fileWatcher;
        public Xv2FileIO fileIO => FileManager.Instance.fileIO;
        public Language PreferedLanguage = Language.English;
        public bool IsInitialized = false;


        #region Initialization
        private Xenoverse2()
        {
        }

        public void Init()
        {
            try
            {
                fileWatcher.ClearAll();
                IsInitialized = false;
                FileManager.Instance.Init();

                if (loadCharacters)
                    InitCharacters();

                if (loadSkills)
                    InitSkills();

                if (loadCmn)
                    InitCmn();
            }
            finally
            {
                IsInitialized = true;
            }
        }

        private void InitCharacters()
        {
            if(fileWatcher.WasFileModified(fileIO.PathInGameDir(CMS_PATH)) || cmsFile == null)
            {
                cmsFile = (CMS_File)FileManager.Instance.GetParsedFileFromGame(CMS_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CMS_PATH));
            }

            if(fileWatcher.WasFileModified(fileIO.PathInGameDir(ERS_PATH)) || ersFile == null)
            {
                ersFile = (ERS_File)FileManager.Instance.GetParsedFileFromGame(ERS_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(ERS_PATH));
            }

            if(fileWatcher.WasFileModified(fileIO.PathInGameDir(CSO_PATH)) || csoFile == null)
            {
                csoFile = (CSO_File)FileManager.Instance.GetParsedFileFromGame(CSO_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CSO_PATH));
            }

            if (fileWatcher.WasFileModified(fileIO.PathInGameDir(PSC_PATH)) || pscFile == null)
            {
                pscFile = (PSC_File)FileManager.Instance.GetParsedFileFromGame(PSC_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(PSC_PATH));
            }

            LoadMsgFiles(ref charaNameMsgFile, CHARACTER_MSG_PATH);

        }

        private void InitSkills()
        {
            if(fileWatcher.WasFileModified(fileIO.PathInGameDir(CUS_PATH)) || cusFile == null)
            {
                cusFile = (CUS_File)FileManager.Instance.GetParsedFileFromGame(CUS_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CUS_PATH));
            }

            if (fileWatcher.WasFileModified(fileIO.PathInGameDir(CMS_PATH)) || cmsFile == null)
            {
                cmsFile = (CMS_File)FileManager.Instance.GetParsedFileFromGame(CMS_PATH); 
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CMS_PATH));
            }

            if (fileWatcher.WasFileModified(fileIO.PathInGameDir(SKILL_IDB_PATH)) || skillIdbFile == null)
            {
                skillIdbFile = (IDB_File)FileManager.Instance.GetParsedFileFromGame(SKILL_IDB_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(SKILL_IDB_PATH));
            }

            if (fileWatcher.WasFileModified(fileIO.PathInGameDir(PUP_PATH)) || pupFile == null)
            {
                pupFile = (PUP_File)FileManager.Instance.GetParsedFileFromGame(PUP_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(PUP_PATH));
            }

            LoadMsgFiles(ref spaSkillNameMsgFile, SUPER_SKILL_MSG_PATH);
            LoadMsgFiles(ref spaSkillDescMsgFile, SUPER_SKILL_DESC_MSG_PATH);
            LoadMsgFiles(ref ultSkillNameMsgFile, ULT_SKILL_MSG_PATH);
            LoadMsgFiles(ref ultSkillDescMsgFile, ULT_SKILL_DESC_MSG_PATH);
            LoadMsgFiles(ref evaSkillNameMsgFile, EVASIVE_SKILL_MSG_PATH);
            LoadMsgFiles(ref evaSkillDescMsgFile, EVASIVE_SKILL_DESC_MSG_PATH);
            LoadMsgFiles(ref metSkillNameMsgFile, AWOKEN_SKILL_MSG_PATH);
            LoadMsgFiles(ref metSkillDescMsgFile, AWOKEN_SKILL_DESC_MSG_PATH);
            LoadMsgFiles(ref btlHudMsgFile, BTLHUD_MSG_PATH);
        }

        private void InitCmn()
        {
            if(fileWatcher.WasFileModified(fileIO.PathInGameDir(CMN_BAC_PATH)) || CmnBac == null)
            {
                CmnBac = (BAC_File)FileManager.Instance.GetParsedFileFromGame(CMN_BAC_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CMN_BAC_PATH));
            }

            if (fileWatcher.WasFileModified(fileIO.PathInGameDir(CMN_BDM_PATH)) || CmnBdm == null)
            {
                CmnBdm = (BDM_File)FileManager.Instance.GetParsedFileFromGame(CMN_BDM_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CMN_BDM_PATH));
            }

            if (fileWatcher.WasFileModified(fileIO.PathInGameDir(CMN_EAN_PATH)) || CmnEan == null)
            {
                CmnEan = (EAN_File)FileManager.Instance.GetParsedFileFromGame(CMN_EAN_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CMN_EAN_PATH));
            }

            if (fileWatcher.WasFileModified(fileIO.PathInGameDir(CMN_CAM_EAN_PATH)) || CmnCamEan == null)
            {
                CmnCamEan = (EAN_File)FileManager.Instance.GetParsedFileFromGame(CMN_CAM_EAN_PATH);
                fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(CMN_CAM_EAN_PATH));
            }

        }

        private void LoadMsgFiles(ref MSG_File[] msgFiles, string path)
        {
            if (msgFiles == null)
                msgFiles = new MSG_File[(int)Language.NumLanguages];

            for (int i = 0; i < (int)Language.NumLanguages; i++)
            {
                string msgPath = path + LanguageSuffix[i];

                if (fileWatcher.WasFileModified(fileIO.PathInGameDir(msgPath)) || msgFiles[i] == null)
                {
                    msgFiles[i] = (MSG_File)FileManager.Instance.GetParsedFileFromGame(msgPath);
                    fileWatcher.FileLoadedOrSaved(fileIO.PathInGameDir(msgPath));
                }
            }

            MSG_File.SynchronizeMsgFiles(msgFiles);
        }

        /// <summary>
        /// Reload skill related files if needed (such as if a file was modified by an external program)
        /// </summary>
        public void RefreshSkills()
        {
            loadSkills = true;
            InitSkills();
        }

        /// <summary>
        /// Reload character related files if needed (such as if a file was modified by an external program)
        /// </summary>
        public void RefreshCharacters()
        {
            loadCharacters = true;
            InitCharacters();
        }

        #endregion

        #region Skill
        public Xv2Skill GetSkill(CUS_File.SkillType skillType, int id1, bool loadFiles = true)
        {
            if (!loadSkills) throw new InvalidOperationException("Xenoverse2.GetSkill: Cannot get skill as skills have not been loaded.");

            var cusEntry = GetSkillCusEntry(skillType, id1);
            if (cusEntry == null) throw new InvalidOperationException($"Xenoverse2.GetSkill: Skill was not found in the system (ID: {id1}, Type: {skillType}).");
            
            var skill = new Xv2Skill()
            {
                skillType = skillType,
                BtlHud = GetAwokenStageNames(cusEntry.ID2, cusEntry.NumTransformations),
                CusEntry = cusEntry,
                Description = GetSkillDescs(skillType, cusEntry.ID2),
                Name = GetSkillNames(skillType, cusEntry.ID2),
                IdbEntry = GetSkillIdbEntry(skillType, cusEntry.ID2),
                PupEntries = new ObservableCollection<PUP_Entry>(pupFile.GetSequence(cusEntry.PUP, cusEntry.NumTransformations)),
                Files = GetSkillFiles(cusEntry, skillType, loadFiles)
            };

            skill.CreateDefaultFiles();

            return skill;
        }

        public Xv2MoveFiles GetSkillFiles(Skill cusEntry, CUS_File.SkillType skillType, bool loadSkillFiles)
        {
            string skillDir = GetSkillDir(skillType);
            string folderName = GetSkillFolderName(cusEntry);
            Xv2MoveFiles moveFiles = new Xv2MoveFiles();
            
            //BAC
            if (cusEntry.FilesLoadedFlags2.HasFlag(Skill.Type.BAC))
            {
                moveFiles.BacPath = String.Format("{0}/{1}/{1}.bac", skillDir, folderName);

                if(loadSkillFiles)
                    moveFiles.BacFile = new Xv2File<BAC_File>((BAC_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BacPath), fileIO.PathInGameDir(moveFiles.BacPath), false, null, false, MoveFileTypes.BAC, 0, true, MoveType.Skill);
            }

            //BCM
            if (cusEntry.FilesLoadedFlags2.HasFlag(Skill.Type.BCM))
            {
                moveFiles.BcmPath = String.Format("{0}/{1}/{1}_PLAYER.bcm", skillDir, folderName);

                if(loadSkillFiles)
                    moveFiles.BcmFile = new Xv2File<BCM_File>((BCM_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BcmPath), fileIO.PathInGameDir(moveFiles.BcmPath), false, null, false, MoveFileTypes.BCM, 0, true, MoveType.Skill);
            }

            //BDM
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.Bdm))
            {
                moveFiles.BdmPath = String.Format("{0}/{1}/{1}_PLAYER.bdm", skillDir, folderName);

                if(loadSkillFiles)
                    moveFiles.BdmFile = new Xv2File<BDM_File>((BDM_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BdmPath), fileIO.PathInGameDir(moveFiles.BdmPath), false, null, false, MoveFileTypes.BDM, 0, true, MoveType.Skill); 
            }

            //BSA + shot.BDM
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.BsaAndShotBdm))
            {
                moveFiles.ShotBdmPath = String.Format("{0}/{1}/{1}_PLAYER.shot.bdm", skillDir, folderName);
                moveFiles.BsaPath = String.Format("{0}/{1}/{1}.bsa", skillDir, folderName);

                if (loadSkillFiles)
                {
                    moveFiles.ShotBdmFile = new Xv2File<BDM_File>((BDM_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.ShotBdmPath), fileIO.PathInGameDir(moveFiles.ShotBdmPath), false, null, false, MoveFileTypes.SHOT_BDM, 0, true, MoveType.Skill);
                    moveFiles.BsaFile = new Xv2File<BSA_File>((BSA_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BsaPath), fileIO.PathInGameDir(moveFiles.BsaPath), false, null, false, MoveFileTypes.BSA, 0, true, MoveType.Skill);
                }
            }

            //BAS
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.Bas))
            {
                moveFiles.BasPath = String.Format("{0}/{1}/{1}.bas", skillDir, folderName);

                if(loadSkillFiles)
                    moveFiles.BasFile = new Xv2File<BAS_File>((BAS_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BasPath), fileIO.PathInGameDir(moveFiles.BasPath), false, null, false, MoveFileTypes.BAS, 0, true, MoveType.Skill);
            }

            //EEPK
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.Eepk))
            {
                if (!cusEntry.HasEepkPath)
                {
                    moveFiles.EepkPath = String.Format("{0}/{1}/{1}.eepk", skillDir, folderName);

                    if(loadSkillFiles)
                        moveFiles.EepkFile = new Xv2File<EffectContainerFile>((EffectContainerFile)FileManager.Instance.GetParsedFileFromGame(moveFiles.EepkPath), fileIO.PathInGameDir(moveFiles.EepkPath), false, null, false, MoveFileTypes.EEPK, 0, true, MoveType.Skill);
                }
                else
                {
                    //This skill uses another skills EEPK, so we dont have to calculate its folder name
                    moveFiles.EepkPath = String.Format("skill/{0}/{1}.eepk", cusEntry.EepkPath, Path.GetFileName(cusEntry.EepkPath));

                    if(loadSkillFiles)
                        moveFiles.EepkFile = new Xv2File<EffectContainerFile>((EffectContainerFile)FileManager.Instance.GetParsedFileFromGame(moveFiles.EepkPath), fileIO.PathInGameDir(moveFiles.EepkPath), true, null, false, MoveFileTypes.EEPK, 0, true, MoveType.Skill);
                }
            }

            //SE ACB
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.CharaSE))
            {
                if (!cusEntry.HasSeAcbPath)
                {
                    moveFiles.SeAcbPath = string.Format(@"sound/SE/Battle/Skill/CAR_BTL_{2}{1}_{0}_SE.acb", cusEntry.ShortName, cusEntry.ID2.ToString("D3"), GetAcbSkillTypeLetter(skillType));

                    if (loadSkillFiles)
                        moveFiles.AddSeAcbFile((ACB_Wrapper)FileManager.Instance.GetParsedFileFromGame(moveFiles.SeAcbPath), -1, fileIO.PathInGameDir(moveFiles.SeAcbPath), false, true, MoveType.Skill);
                }
                else
                {
                    moveFiles.SeAcbPath = string.Format(@"sound/SE/Battle/Skill/{0}.acb", cusEntry.SePath);

                    if(loadSkillFiles)
                        moveFiles.AddSeAcbFile((ACB_Wrapper)FileManager.Instance.GetParsedFileFromGame(moveFiles.SeAcbPath), -1, fileIO.PathInGameDir(moveFiles.SeAcbPath), true, true, MoveType.Skill);
                }
            }

            //VOX ACB
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.CharaVOX))
            {
                //Japanese
                string[] files = fileIO.GetFilesInDirectory("sound/VOX/Battle/Skill", "acb");
                string name = (!cusEntry.HasVoxAcbPath) ? string.Format(@"CAR_BTL_{2}{1}_{0}_", cusEntry.ShortName, cusEntry.ID2.ToString("D3"), GetAcbSkillTypeLetter(skillType)) : cusEntry.VoxPath;
                
                foreach(var file in files.Where(f => f.Contains(name) && f.Contains("_VOX.acb")))
                {
                    string[] split = Path.GetFileNameWithoutExtension(file).Split('_');
                    string charaSuffix = split[(split.Length - 2 > 0) ? split.Length - 2 : 0];
                    moveFiles.VoxAcbPath.Add(file);

                    if(loadSkillFiles)
                        moveFiles.AddVoxAcbFile((ACB_Wrapper)FileManager.Instance.GetParsedFileFromGame(file), charaSuffix, false, fileIO.PathInGameDir(file), cusEntry.HasVoxAcbPath, false, MoveType.Skill);
                }

                //English
                files = fileIO.GetFilesInDirectory("sound/VOX/Battle/Skill/en", "acb");

                foreach (var file in files.Where(f => f.Contains(name) && f.Contains("_VOX.acb")))
                {
                    string[] split = Path.GetFileNameWithoutExtension(file).Split('_');
                    string charaSuffix = split[(split.Length - 2 > 0) ? split.Length - 2 : 0];
                    moveFiles.VoxAcbPath.Add(file);

                    if(loadSkillFiles)
                        moveFiles.AddVoxAcbFile((ACB_Wrapper)FileManager.Instance.GetParsedFileFromGame(file), charaSuffix, true, fileIO.PathInGameDir(file), cusEntry.HasVoxAcbPath, false, MoveType.Skill);
                }
            }

            //EAN
            if (cusEntry.FilesLoadedFlags2.HasFlag(Skill.Type.EAN))
            {
                string name = (!cusEntry.HasEanPath) ? string.Format("{0}/{1}/{1}.ean", skillDir, folderName) : Utils.ResolveRelativePath("skill/" + cusEntry.EanPath + ".ean");
                name = Utils.SanitizePath(name);
                string[] files = fileIO.GetFilesInDirectory(Path.GetDirectoryName(name), ".ean");

                foreach (var file in files.Where(f => f.Contains(Path.GetFileNameWithoutExtension(name)) && f.Contains(".ean") && !f.Contains(".cam")))
                {
                    string[] split = Path.GetFileNameWithoutExtension(file).Split('_');
                    string charaSuffix = (split.Length == 4) ? split[3].Split('.')[0] : null;
                    moveFiles.EanPaths.Add(file);

                    if(loadSkillFiles)
                        moveFiles.AddEanFile((EAN_File)FileManager.Instance.GetParsedFileFromGame(file), charaSuffix, fileIO.PathInGameDir(file), cusEntry.HasEanPath, string.IsNullOrWhiteSpace(charaSuffix), MoveType.Skill);
                }

                //Create default EAN if none was loaded (duplicate chara-unique one)
                if (!moveFiles.EanFile.Any(x => x.IsDefault) && loadSkillFiles)
                    moveFiles.AddEanFile(moveFiles.EanFile[0].File.Copy(), null, name);
            }

            //CAM
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.CamEan))
            {
                string nameWithoutExt = (!cusEntry.HasCamEanPath) ? string.Format("{0}/{1}/{1}", skillDir, folderName) : Utils.ResolveRelativePath("skill/" + cusEntry.CamEanPath);
                nameWithoutExt = Utils.SanitizePath(nameWithoutExt);
                string name = nameWithoutExt + ".cam.ean";
                string[] files = fileIO.GetFilesInDirectory(Path.GetDirectoryName(nameWithoutExt), ".ean");

                foreach (var file in files.Where(f => f.Contains(nameWithoutExt) && f.Contains("cam.ean")))
                {
                    string[] split = Path.GetFileNameWithoutExtension(file).Split('_');
                    string charaSuffix = (split.Length == 4) ? split[3].Split('.')[0] : null;
                    moveFiles.CamPaths.Add(file);

                    if(loadSkillFiles)
                        moveFiles.AddCamEanFile((EAN_File)FileManager.Instance.GetParsedFileFromGame(file), charaSuffix, fileIO.PathInGameDir(file), cusEntry.HasCamEanPath, string.IsNullOrWhiteSpace(charaSuffix), MoveType.Skill);
                }

                //Create default CAM.EAN if none was loaded (duplicate chara-unique one)
                if (!moveFiles.CamEanFile.Any(x => x.IsDefault) && loadSkillFiles)
                    moveFiles.AddCamEanFile(moveFiles.CamEanFile[0].File.Copy(), null, name);
            }
            
            //AFTER BAC
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.AfterBac))
            {
                if (!cusEntry.HasAfterBacPath)
                {
                    moveFiles.AfterBacPath = String.Format("{0}/{1}/{1}_AFTER.bac", skillDir, folderName);

                    if (loadSkillFiles)
                        moveFiles.AfterBacFile = new Xv2File<BAC_File>((BAC_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.AfterBacPath), fileIO.PathInGameDir(moveFiles.AfterBacPath), false, null, false, MoveFileTypes.BAC, 0, false, MoveType.Skill);
                }
                else
                {
                    //moveFiles.AfterBacPath = String.Format("skill/{0}/{1}.bac", cusEntry.AfterBacPath, Path.GetFileName(cusEntry.AfterBacPath));
                    moveFiles.AfterBacPath = String.Format("skill/{0}.bac", cusEntry.AfterBacPath);

                    if (loadSkillFiles)
                        moveFiles.AfterBacFile = new Xv2File<BAC_File>((BAC_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.AfterBacPath), fileIO.PathInGameDir(moveFiles.AfterBacPath), true, null, false, MoveFileTypes.BAC, 0, false, MoveType.Skill);
                }
            }

            //AFTER BCM
            if (cusEntry.FilesLoadedFlags1.HasFlag(Skill.FilesLoadedFlags.AfterBcm))
            {
                if (!cusEntry.HasAfterBacPath)
                {
                    moveFiles.AfterBcmPath = String.Format("{0}/{1}/{1}_AFTER_PLAYER.bcm", skillDir, folderName);

                    if(loadSkillFiles)
                        moveFiles.AfterBcmFile = new Xv2File<BCM_File>((BCM_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.AfterBcmPath), fileIO.PathInGameDir(moveFiles.AfterBcmPath), false, null, false, MoveFileTypes.BCM, 0, false, MoveType.Skill);
                }
                else
                {
                    //moveFiles.AfterBcmPath = String.Format("skill/{0}/{1}.bcm", cusEntry.AfterBcmPath, Path.GetFileName(cusEntry.AfterBcmPath));
                    moveFiles.AfterBcmPath = String.Format("skill/{0}.bcm", cusEntry.AfterBcmPath);

                    if (loadSkillFiles)
                        moveFiles.AfterBcmFile = new Xv2File<BCM_File>((BCM_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.AfterBcmPath), fileIO.PathInGameDir(moveFiles.AfterBcmPath), true, null, false, MoveFileTypes.BCM, 0, false, MoveType.Skill);
                }
            }



            return moveFiles;
        }

        public Skill GetSkillCusEntry(CUS_File.SkillType skillType, int id1)
        {
            switch (skillType)
            {
                case CUS_File.SkillType.Super:
                    return cusFile.SuperSkills.FirstOrDefault(s => s.ID1 == id1);
                case CUS_File.SkillType.Ultimate:
                    return cusFile.UltimateSkills.FirstOrDefault(s => s.ID1 == id1);
                case CUS_File.SkillType.Evasive:
                    return cusFile.EvasiveSkills.FirstOrDefault(s => s.ID1 == id1);
                case CUS_File.SkillType.Blast:
                    return cusFile.BlastSkills.FirstOrDefault(s => s.ID1 == id1);
                case CUS_File.SkillType.Awoken:
                    return cusFile.AwokenSkills.FirstOrDefault(s => s.ID1 == id1);
                default:
                    throw new InvalidDataException("GetSkill: unknown skilltype = " + skillType);
            }
        }

        public IDB_Entry GetSkillIdbEntry(CUS_File.SkillType skillType, int id2)
        {
            return skillIdbFile.Entries.FirstOrDefault(i => i.ID == id2 && i.Type == (int)skillType);
        }
        
        public List<Xv2Item> GetSkillList(CUS_File.SkillType skillType)
        {
            List<Xv2Item> items = new List<Xv2Item>();

            foreach(var skill in cusFile.GetSkills(skillType))
            {
                if(skillType == CUS_File.SkillType.Blast)
                {
                    items.Add(new Xv2Item(skill.ID1, skill.ShortName));
                }
                else
                {
                    items.Add(new Xv2Item(skill.ID1, GetSkillName(skillType, skill.ID2, PreferedLanguage)));
                }
            }

            return items;
        }

        //Helpers
        public string GetSkillDir(CUS_File.SkillType skillType)
        {
            switch (skillType)
            {
                case CUS_File.SkillType.Super:
                    return "skill/SPA";
                case CUS_File.SkillType.Ultimate:
                    return "skill/ULT";
                case CUS_File.SkillType.Evasive:
                    return "skill/ESC";
                case CUS_File.SkillType.Blast:
                    return "skill/BLT";
                case CUS_File.SkillType.Awoken:
                    return "skill/MET";
                default:
                    return null;
            }
        }

        public string GetSkillFolderName(Skill cusEntry)
        {
            int cmsId = (int)Math.Floor(cusEntry.ID2 / 10f);
            string charaShortName = cmsFile.GetEntry(cmsId.ToString()).Str_04;

            //If chara ID belongs to a CAC, the skill is tagged as CMN instead.
            if (cmsId >= 100 && cmsId < 109)
                charaShortName = "CMN";

            return String.Format("{0}_{1}_{2}", cusEntry.ID2.ToString("D3"), charaShortName, cusEntry.ShortName);
        }

        public string GetSkillName(CUS_File.SkillType skillType, int id2, Language language)
        {
            MSG_File[] msgFiles = GetSkillNameMsgFile(skillType);
            return msgFiles[(int)language].GetSkillName(id2, skillType);
        }

        public string[] GetSkillDescs(CUS_File.SkillType skillType, int id2)
        {
            if (skillType == CUS_File.SkillType.Blast) return new string[(int)Language.NumLanguages];
            string[] descs = new string[(int)Language.NumLanguages];
            MSG_File[] msgFiles = GetSkillDescMsgFile(skillType);

            for (int i = 0; i < (int)Language.NumLanguages; i++)
            {
                descs[i] = msgFiles[i].GetSkillDesc(id2, skillType);
            }

            return descs;
        }

        public string[] GetSkillNames(CUS_File.SkillType skillType, int id2)
        {
            string[] names = new string[(int)Language.NumLanguages];

            if(skillType == CUS_File.SkillType.Blast)
            {
                for(int i = 0; i < names.Length; i++)
                {
                    names[i] = cusFile.BlastSkills.FirstOrDefault(x => x.ID2 == id2).ShortName;
                }

                return names;
            }

            MSG_File[] msgFiles = GetSkillNameMsgFile(skillType);

            for (int i = 0; i < (int)Language.NumLanguages; i++)
            {
                names[i] = msgFiles[i].GetSkillName(id2, skillType);
            }

            return names;
        }

        public ObservableCollection<string[]> GetAwokenStageNames(int id2, int numStages)
        {
            if (numStages > 3) numStages = 1;
            ObservableCollection<string[]> stages = new ObservableCollection<string[]>();

            for(int a = 0; a < numStages; a++)
            {
                string[] names = new string[(int)Language.NumLanguages];

                for (int i = 0; i < (int)Language.NumLanguages; i++)
                {
                    names[i] = btlHudMsgFile[i].GetAwokenStageName(id2, a);
                }

                stages.Add(names);
            }

            return stages;
        }

        private MSG_File[] GetSkillNameMsgFile(CUS_File.SkillType skillType)
        {
            switch (skillType)
            {
                case CUS_File.SkillType.Super:
                    return spaSkillNameMsgFile;
                case CUS_File.SkillType.Ultimate:
                    return ultSkillNameMsgFile;
                case CUS_File.SkillType.Evasive:
                    return evaSkillNameMsgFile;
                case CUS_File.SkillType.Awoken:
                    return metSkillNameMsgFile;
                default:
                    throw new InvalidDataException("Unknown skilltype = " + skillType);
            }
        }

        private MSG_File[] GetSkillDescMsgFile(CUS_File.SkillType skillType)
        {
            switch (skillType)
            {
                case CUS_File.SkillType.Super:
                    return spaSkillDescMsgFile;
                case CUS_File.SkillType.Ultimate:
                    return ultSkillDescMsgFile;
                case CUS_File.SkillType.Evasive:
                    return evaSkillDescMsgFile;
                case CUS_File.SkillType.Awoken:
                    return metSkillDescMsgFile;
                default:
                    throw new InvalidDataException("Unknown skilltype = " + skillType);
            }
        }

        public string GetAcbSkillTypeLetter(CUS_File.SkillType skillType)
        {
            switch (skillType)
            {
                case CUS_File.SkillType.Super:
                    return "S";
                case CUS_File.SkillType.Ultimate:
                    return "U";
                case CUS_File.SkillType.Evasive:
                    return "E";
                case CUS_File.SkillType.Blast:
                    return "B";
                case CUS_File.SkillType.Awoken:
                    return "M";
            }

            return null;
        }


        #endregion

        #region Character
        public Xv2Character GetCharacter(int cmsId, bool loadFiles = true)
        {
            if (!loadCharacters) throw new InvalidOperationException("Xenoverse2.GetCharacter: Cannot get character as characters have not been loaded.");

            var cmsEntry = cmsFile.CMS_Entries.FirstOrDefault(x => x.ID == cmsId);
            if (cmsEntry == null) throw new InvalidOperationException($"Xenoverse2.GetCharacter: Character was not found in the system (ID: {cmsId}).");
            var names = (IsCac(cmsId)) ? new string[1] { GetCacRaceName(cmsId) } : GetCharacterName(cmsEntry.ShortName);
            var csoEntries = csoFile.CsoEntries.Where(x => x.CharaID == cmsId).ToList();
            var ersEntry = ersFile.GetEntry(2, cmsId);

            List<string> loadedFiles = new List<string>();

            if (loadFiles)
            {
                //Load bcs
                string bcsPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}.bcs", cmsEntry.ShortName, cmsEntry.BcsPath));
                BCS_File bcsFile = (BCS_File)FileManager.Instance.GetParsedFileFromGame(bcsPath);

                //Load bai file
                string baiPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}.bai", cmsEntry.ShortName, cmsEntry.BaiPath));
                BAI_File baiFile = (BAI_File)FileManager.Instance.GetParsedFileFromGame(baiPath);

                //Load esk file
                string eskPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}_000.esk", cmsEntry.ShortName, cmsEntry.BcsPath));
                ESK_File eskFile = (ESK_File)FileManager.Instance.GetParsedFileFromGame(eskPath);

                //Costumes
                List<Xv2File<AMK_File>> amkFiles = new List<Xv2File<AMK_File>>();
                AsyncObservableCollection<Xv2CharaCostume> costumes = new AsyncObservableCollection<Xv2CharaCostume>();

                foreach(var csoEntry in csoEntries)
                {
                    Xv2CharaCostume costume = Xv2CharaCostume.GetAndAddCostume(costumes, (int)csoEntry.Costume);

                    costume.CsoSkills = csoEntry.SkillCharaCode;

                    //AMK
                    string amkPath = Utils.ResolveRelativePath(string.Format("chara/{0}.amk", csoEntry.AmkPath));

                    if (!string.IsNullOrWhiteSpace(csoEntry.AmkPath) && !loadedFiles.Contains(amkPath))
                    {
                        AMK_File amkFile = (AMK_File)FileManager.Instance.GetParsedFileFromGame(amkPath, false, false);

                        //AMK can be declared in CSO but not actually exist, so we must check. If it is missing then just skip it.
                        if(amkFile != null)
                        {
                            amkFiles.Add(new Xv2File<AMK_File>(amkFile, FileManager.Instance.GetAbsolutePath(amkPath), !Utils.CompareSplitString(csoEntry.AmkPath, '/', 0, cmsEntry.ShortName), null, false, MoveFileTypes.AMK, (int)csoEntry.Costume, true, MoveType.Character));
                            loadedFiles.Add(amkPath);
                        }
                    }
                    else
                    {
                        Xv2File<AMK_File>.AddCostume(amkFiles, FileManager.Instance.GetAbsolutePath(amkPath), (int)csoEntry.Costume, false);
                    }

                    //ACBs loaded in GetCharacterMoveFiles()
                }


                Xv2Character chara = new Xv2Character()
                {
                    CmsEntry = cmsEntry,
                    CsoEntry = csoEntries,
                    ErsEntry = ersEntry,
                    Name = names,
                    BcsFile = new Xv2File<BCS_File>(bcsFile, fileIO.PathInGameDir(bcsPath), false, null, false),
                    AmkFile = amkFiles,
                    BaiFile = new Xv2File<BAI_File>(baiFile, fileIO.PathInGameDir(baiPath), !cmsEntry.IsSelfReference(cmsEntry.BaiPath)),
                    EskFile = new Xv2File<ESK_File>(eskFile, fileIO.PathInGameDir(eskPath), false),
                    MovesetFiles = GetCharacterMoveFiles(cmsEntry, ersEntry, csoEntries, loadFiles),
                    Costumes = costumes
                };

                chara.LoadPartSets();
                chara.CreateDefaultFiles();

                return chara;
            }
            else
            {
                return new Xv2Character()
                {
                    CmsEntry = cmsEntry,
                    CsoEntry = csoEntries,
                    ErsEntry = ersEntry,
                    Name = names,
                    MovesetFiles = GetCharacterMoveFiles(cmsEntry, ersEntry, csoEntries, loadFiles)
                };
            }
        }

        public List<Xv2Item> GetCharacterList()
        {
            if (!loadCharacters) throw new InvalidOperationException("Xenoverse2.GetCharacterList: characters are not loaded.");
            List<Xv2Item> items = new List<Xv2Item>();

            foreach (var character in cmsFile.CMS_Entries)
            {
                string name = (IsCac(character.ID)) ? GetCacRaceName(character.ID) : charaNameMsgFile[(int)PreferedLanguage].GetCharacterName(character.ShortName);

                items.Add(new Xv2Item(character.ID, name));
            }

            return items;
        }

        public List<Xv2Item> GetPartSetList(int cmsId)
        {
            var cmsEntry = cmsFile.CMS_Entries.FirstOrDefault(x => x.ID == cmsId);
            if (cmsEntry == null) throw new InvalidOperationException($"Xenoverse2.GetPartSetList: Character was not found in the system (ID: {cmsId}).");

            string bcsPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}.bcs", cmsEntry.ShortName, cmsEntry.BcsPath));
            BCS_File bcsFile = (BCS_File)FileManager.Instance.GetParsedFileFromGame(bcsPath);

            List<Xv2Item> items = new List<Xv2Item>();

            foreach (var partSet in bcsFile.PartSets)
                items.Add(new Xv2Item(partSet.ID, partSet.ID.ToString()));

            return items;
        }

        public BCS_File GetBcsFile(int cmsId)
        {
            var cmsEntry = cmsFile.CMS_Entries.FirstOrDefault(x => x.ID == cmsId);
            if (cmsEntry == null) throw new InvalidOperationException($"Xenoverse2.GetBcsFile: Character was not found in the system (ID: {cmsId}).");

            string bcsPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}.bcs", cmsEntry.ShortName, cmsEntry.BcsPath));
            return (BCS_File)FileManager.Instance.GetParsedFileFromGame(bcsPath);
        }

        public string[] GetCharacterName(string shortName)
        {
            string[] names = new string[(int)Language.NumLanguages];

            for(int i = 0; i < names.Length; i++)
            {
                names[i] = charaNameMsgFile[i].GetCharacterName(shortName);
                if (string.IsNullOrWhiteSpace(names[i])) names[i] = string.Format("Unknown Character - {0}", shortName);
            }

            return names;
        }

        public string[] GetCharacterName(int id)
        {
            if (!loadCharacters) throw new InvalidOperationException("Xenoverse2.GetCharacterName: Characters have not been loaded.");

            string shortName = cmsFile.CMS_Entries.FirstOrDefault(x => x.ID == id)?.ShortName;
            return GetCharacterName(shortName);
        }

        public string GetCharacterName(int id, Language language)
        {
            return GetCharacterName(id)[(int)language];
        }

        public string GetCharacterName(string shortName, Language language)
        {
            return GetCharacterName(shortName)[(int)language];
        }

        private Xv2MoveFiles GetCharacterMoveFiles(CMS_Entry cmsEntry, ERS_MainTableEntry ersEntry, IList<CSO_Entry> csoEntries, bool loadFiles)
        {
            List<string> loadedFiles = new List<string>();
            Xv2MoveFiles moveFiles = new Xv2MoveFiles();

            //Clear defaults out
            moveFiles.EanFile.Clear();
            moveFiles.CamEanFile.Clear();

            //BAC
            moveFiles.BacPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}_PLAYER.bac", cmsEntry.ShortName, cmsEntry.BacPath));

            if(loadFiles)
                moveFiles.BacFile = new Xv2File<BAC_File>((BAC_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BacPath), fileIO.PathInGameDir(moveFiles.BacPath), !cmsEntry.IsSelfReference(cmsEntry.BacPath), null, false, MoveFileTypes.BAC, 0, true, MoveType.Character);

            //BCM
            moveFiles.BcmPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}_PLAYER.bcm", cmsEntry.ShortName, cmsEntry.BcmPath));

            if(loadFiles)
                moveFiles.BcmFile = new Xv2File<BCM_File>((BCM_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BcmPath), fileIO.PathInGameDir(moveFiles.BcmPath), !cmsEntry.IsSelfReference(cmsEntry.BcmPath), null, false, MoveFileTypes.BCM, 0, true, MoveType.Character);

            //EAN
            string eanPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}.ean", cmsEntry.ShortName, cmsEntry.EanPath));
            moveFiles.EanPaths.Add(eanPath);
            moveFiles.EanFile.Clear();

            if(loadFiles)
                moveFiles.EanFile.Add(new Xv2File<EAN_File>((EAN_File)FileManager.Instance.GetParsedFileFromGame(eanPath), fileIO.PathInGameDir(eanPath), !cmsEntry.IsSelfReference(cmsEntry.EanPath), null, false, MoveFileTypes.EAN, 0, true, MoveType.Character));

            //CAM
            if (!string.IsNullOrWhiteSpace(cmsEntry.CamEanPath))
            {
                string camEanPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}.cam.ean", cmsEntry.ShortName, cmsEntry.CamEanPath));
                moveFiles.CamPaths.Add(camEanPath);
                moveFiles.CamEanFile.Clear();

                if (loadFiles)
                    moveFiles.CamEanFile.Add(new Xv2File<EAN_File>((EAN_File)FileManager.Instance.GetParsedFileFromGame(camEanPath), fileIO.PathInGameDir(camEanPath), !cmsEntry.IsSelfReference(cmsEntry.CamEanPath), null, false, MoveFileTypes.CAM_EAN, 0, true, MoveType.Character));
            }

            //BDM
            if (!string.IsNullOrWhiteSpace(cmsEntry.BdmPath))
            {
                moveFiles.BdmPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}_PLAYER.bdm", cmsEntry.ShortName, cmsEntry.BdmPath));

                if(loadFiles)
                    moveFiles.BdmFile = new Xv2File<BDM_File>((BDM_File)FileManager.Instance.GetParsedFileFromGame(moveFiles.BdmPath), fileIO.PathInGameDir(moveFiles.BdmPath), !cmsEntry.IsSelfReference(cmsEntry.BdmPath), null, false, MoveFileTypes.BDM, 0, true, MoveType.Character);

            }
            //EEPK
            if (ersEntry != null)
            {
                bool borrowed = !Utils.CompareSplitString(ersEntry.FILE_PATH, '/', 1, cmsEntry.ShortName);
                moveFiles.EepkPath = string.Format("vfx/{0}", ersEntry.FILE_PATH);

                if(loadFiles)
                    moveFiles.EepkFile = new Xv2File<EffectContainerFile>((EffectContainerFile)FileManager.Instance.GetParsedFileFromGame(moveFiles.EepkPath), fileIO.PathInGameDir(moveFiles.EepkPath), borrowed, null, false, MoveFileTypes.EEPK, 0, true, MoveType.Character);
            }

            //ACBs
            if(csoEntries.Count > 0)
            {

                foreach (var csoEntry in csoEntries)
                {
                    moveFiles.SeAcbPath = $"sound/SE/Battle/Chara/{csoEntry.SePath}.acb";
                    string acbPath = $"sound/VOX/Battle/Chara/{csoEntry.VoxPath}.acb";
                    string acbEngPath = $"sound/VOX/Battle/Chara/en/{csoEntry.VoxPath}.acb";

                    //SE
                    if (csoEntry.HasSePath && !loadedFiles.Contains(moveFiles.SeAcbPath))
                    {
                        bool borrowed = !Utils.CompareSplitString(csoEntry.SePath, '_', 2, cmsEntry.ShortName);

                        if(loadFiles)
                            moveFiles.SeAcbFile.Add(new Xv2File<ACB_Wrapper>((ACB_Wrapper)FileManager.Instance.GetParsedFileFromGame(moveFiles.SeAcbPath), fileIO.PathInGameDir(moveFiles.SeAcbPath), borrowed, null, false, MoveFileTypes.SE_ACB, (int)csoEntry.Costume, true, MoveType.Character));

                        loadedFiles.Add(moveFiles.SeAcbPath);
                    }
                    else
                    {
                        Xv2File<ACB_Wrapper>.AddCostume(moveFiles.SeAcbFile, FileManager.Instance.GetAbsolutePath(moveFiles.SeAcbPath), (int)csoEntry.Costume, false);
                    }

                    if(cmsEntry.ID >= 100 && cmsEntry.ID <= 107)
                    {
                        //CaC
                        for(int i = 0; i <= 14; i++)
                        {
                            switch ((CustomCharacter)cmsEntry.ID)
                            {
                                case CustomCharacter.HUF:
                                case CustomCharacter.SYF:
                                    LoadCaCVox(false, i, moveFiles, true, loadedFiles, loadFiles);
                                    LoadCaCVox(false, i, moveFiles, false, loadedFiles, loadFiles);
                                    break;
                                case CustomCharacter.HUM:
                                case CustomCharacter.SYM:
                                case CustomCharacter.NMC:
                                case CustomCharacter.FRI:
                                case CustomCharacter.MAM:
                                    LoadCaCVox(true, i, moveFiles, true, loadedFiles, loadFiles);
                                    LoadCaCVox(true, i, moveFiles, false, loadedFiles, loadFiles);
                                    break;
                                case CustomCharacter.MAP:
                                    LoadCaCVox(true, i, moveFiles, true, loadedFiles, loadFiles); //Male (En)
                                    LoadCaCVox(false, i, moveFiles, true, loadedFiles, loadFiles); //Female (En)
                                    LoadCaCVox(true, i, moveFiles, false, loadedFiles, loadFiles); //Male (Jp)
                                    LoadCaCVox(false, i, moveFiles, false, loadedFiles, loadFiles); //Female (Jp)
                                    break;

                            }
                        }
                    }
                    else
                    {
                        //Regular roster character
                        //VOX, Jap
                        LoadCharaVox(cmsEntry.ShortName, csoEntry, moveFiles, false, loadedFiles, loadFiles);

                        //VOX, Eng
                        LoadCharaVox(cmsEntry.ShortName, csoEntry, moveFiles, true, loadedFiles, loadFiles);
                    }
                }
            }

            //Load fce ean
            string fceEanPath = Utils.ResolveRelativePath(string.Format("chara/{0}/{1}.fce.ean", cmsEntry.ShortName, cmsEntry.FceEanPath));
            moveFiles.EanPaths.Add(fceEanPath);
            moveFiles.EanFile.Add(new Xv2File<EAN_File>((EAN_File)FileManager.Instance.GetParsedFileFromGame(fceEanPath), fileIO.PathInGameDir(fceEanPath), !cmsEntry.IsSelfReference(cmsEntry.FceEanPath), null, false, MoveFileTypes.FCE_EAN, 0, true, MoveType.Character));

            return moveFiles;
        }

        private void LoadCharaVox(string shortName, CSO_Entry csoEntry, Xv2MoveFiles moveFiles, bool english, List<string> loadedFiles, bool loadFiles)
        {
            string acbPath = (english) ? $"sound/VOX/Battle/Chara/en/{csoEntry.VoxPath}.acb" : $"sound/VOX/Battle/Chara/{csoEntry.VoxPath}.acb";

            if (csoEntry.HasVoxPath && !loadedFiles.Contains(acbPath))
            {
                bool borrowed = !Utils.CompareSplitString(csoEntry.VoxPath, '_', 2, shortName);

                moveFiles.VoxAcbPath.Add(acbPath);

                if (loadFiles)
                    moveFiles.VoxAcbFile.Add(new Xv2File<ACB_Wrapper>((ACB_Wrapper)FileManager.Instance.GetParsedFileFromGame(acbPath), fileIO.PathInGameDir(acbPath), borrowed, null, english, MoveFileTypes.VOX_ACB, (int)csoEntry.Costume, true, MoveType.Character));

                loadedFiles.Add(acbPath);
            }
            else
            {
                Xv2File<ACB_Wrapper>.AddCostume(moveFiles.VoxAcbFile, FileManager.Instance.GetAbsolutePath(acbPath), (int)csoEntry.Costume, false);
            }
        }

        private void LoadCaCVox(bool isMale, int id, Xv2MoveFiles moveFiles, bool english, List<string> loadedFiles, bool loadFiles)
        {
            string acbPath;

            if (isMale)
                acbPath = (english) ? $"sound/VOX/Battle/Chara/en/CAR_BTL_M{id.ToString("D2")}_VOX.acb" : $"sound/VOX/Battle/Chara/CAR_BTL_M{id.ToString("D2")}_VOX.acb";
            else
                acbPath = (english) ? $"sound/VOX/Battle/Chara/en/CAR_BTL_F{id.ToString("D2")}_VOX.acb" : $"sound/VOX/Battle/Chara/CAR_BTL_F{id.ToString("D2")}_VOX.acb";

            moveFiles.VoxAcbPath.Add(acbPath);

            if (loadFiles)
            {
                moveFiles.VoxAcbFile.Add(new Xv2File<ACB_Wrapper>((ACB_Wrapper)FileManager.Instance.GetParsedFileFromGame(acbPath), fileIO.PathInGameDir(acbPath), false, "HUM", english, MoveFileTypes.VOX_ACB, id, true, MoveType.Character));
            }

            loadedFiles.Add(acbPath);
        }
        
        public static string GetCacRaceName(int cmsId)
        {
            switch ((CustomCharacter)cmsId)
            {
                case CustomCharacter.HUM: 
                    return "Human Male";
                case CustomCharacter.HUF: 
                    return "Human Female";
                case CustomCharacter.SYM: 
                    return "Saiyan Male";
                case CustomCharacter.SYF: 
                    return "Saiyan Female";
                case CustomCharacter.NMC: 
                    return "Namekian";
                case CustomCharacter.FRI: 
                    return "Frieza Race";
                case CustomCharacter.MAM: 
                    return "Majin Male";
                case CustomCharacter.MAF: 
                    return "Majin Female";
                case CustomCharacter.MAP:
                    return "Majin Purification";
            }

            return null;
        }

        public static bool IsCac(int cmsId)
        {
            return cmsId >= 100 && cmsId <= 108;
        }
        #endregion

        #region Save/Install
        public void SaveSkills()
        {
            if (!loadSkills) throw new InvalidOperationException("Xenoverse2.SaveSkills: loadSkills was null, operation not valid.");

            FileManager.Instance.SaveFileToGame(CUS_PATH, cusFile);
            FileManager.Instance.SaveFileToGame(SKILL_IDB_PATH, skillIdbFile);
            FileManager.Instance.SaveFileToGame(PUP_PATH, pupFile);

            FileManager.Instance.SaveMsgFilesToGame(SUPER_SKILL_MSG_PATH, spaSkillNameMsgFile);
            FileManager.Instance.SaveMsgFilesToGame(SUPER_SKILL_DESC_MSG_PATH, spaSkillDescMsgFile);

            FileManager.Instance.SaveMsgFilesToGame(ULT_SKILL_MSG_PATH, ultSkillNameMsgFile);
            FileManager.Instance.SaveMsgFilesToGame(ULT_SKILL_DESC_MSG_PATH, ultSkillDescMsgFile);

            FileManager.Instance.SaveMsgFilesToGame(EVASIVE_SKILL_MSG_PATH, evaSkillNameMsgFile);
            FileManager.Instance.SaveMsgFilesToGame(EVASIVE_SKILL_DESC_MSG_PATH, evaSkillDescMsgFile);

            FileManager.Instance.SaveMsgFilesToGame(AWOKEN_SKILL_MSG_PATH, metSkillNameMsgFile);
            FileManager.Instance.SaveMsgFilesToGame(AWOKEN_SKILL_DESC_MSG_PATH, metSkillDescMsgFile);
            FileManager.Instance.SaveMsgFilesToGame(BTLHUD_MSG_PATH, btlHudMsgFile);


        }

        /// <summary>
        /// Save a Xv2Skill instance to the game.
        /// </summary>
        public void SaveSkill(Xv2Skill skill)
        {
            //Refresh files
            RefreshSkills();

            //Cus Entry
            skill.CusEntry.ID1 = (ushort)CUS_File.ConvertToID1(skill.CusEntry.ID2, skill.skillType);
            skill.CusEntry.PUP = (skill.PupEntries?.Count > 0) ? InstallPupEntries(skill.PupEntries, skill.CusEntry.ID1) : ushort.MaxValue;
            InstallSkillCusEntry(skill.CusEntry, skill.skillType);

            //IDB Entry
            if(skill.IdbEntry != null)
            {
                skill.IdbEntry.ID = skill.CusEntry.ID2;
                skill.IdbEntry.Type = (ushort)skill.skillType;
                InstallSkillIdbEntry(skill.IdbEntry);
            }

            //Skill files
            skill.CalculateSkillFilePaths();
            skill.UpdateCusFlags();
            skill.SaveMoveFiles();

            //Name and Desc
            InstallSkillName(skill.Name, skill.CusEntry.ID2, skill.skillType);
            InstallSkillDesc(skill.Description, skill.CusEntry.ID2, skill.skillType);

            if (skill.skillType == CUS_File.SkillType.Awoken && skill.BtlHud != null)
                InstallSkillAwokenName(skill.BtlHud, skill.CusEntry.ID2);

            //Save files
            SaveSkills();
        }

        public void SaveCharacters()
        {
            FileManager.Instance.SaveFileToGame(CMS_PATH, cmsFile);
            FileManager.Instance.SaveFileToGame(ERS_PATH, ersFile);
            FileManager.Instance.SaveFileToGame(CSO_PATH, csoFile);
            FileManager.Instance.SaveFileToGame(PSC_PATH, pscFile);
            FileManager.Instance.SaveMsgFilesToGame(CHARACTER_MSG_PATH, charaNameMsgFile);
        }

        public void SaveCharacter(Xv2Character chara, bool isMoveset)
        {
            //Refresh files
            RefreshCharacters();

            //Character files
            chara.CalculateFilePaths();
            InstallErsCharacterEntry(chara.CmsEntry.ShortName, chara.CmsEntry.ID, !chara.MovesetFiles.EepkFile.File.IsNull(), chara.MovesetFiles.EepkFile.Borrowed);
            InstallCmsEntry(chara.CmsEntry);
            InstallCharacterCostumes(chara);

            if (!isMoveset)
            {
                //Name
                InstallCharacterName(chara.Name, chara.CmsEntry.ShortName);
            }

            //Save
            chara.SaveFiles();
            SaveCharacters();
        }

        private void InstallCharacterCostumes(Xv2Character chara)
        {
            List<CSO_Entry> csoEntries = new List<CSO_Entry>();

            foreach (var costume in chara.Costumes)
            {
                //Xv2File<ACB_Wrapper> se = chara.MovesetFiles.SeAcbFile.FirstOrDefault(x => x.HasCostume(costume.CostumeId));
                //Xv2File<ACB_Wrapper> vox = chara.MovesetFiles.VoxAcbFile.FirstOrDefault(x => x.HasCostume(costume.CostumeId));
                //Xv2File<AMK_File> amk = chara.AmkFile.FirstOrDefault(x => x.HasCostume(costume.CostumeId));


                if (costume.HasCsoData(chara))
                {
                    Xv2File<ACB_Wrapper> se = Xv2File<ACB_Wrapper>.GetCostumeFileOrDefault(chara.MovesetFiles.SeAcbFile, costume.CostumeId);
                    Xv2File<ACB_Wrapper> vox = Xv2File<ACB_Wrapper>.GetCostumeFileOrDefault(chara.MovesetFiles.VoxAcbFile, costume.CostumeId);
                    Xv2File<AMK_File> amk = Xv2File<AMK_File>.GetCostumeFileOrDefault(chara.AmkFile, costume.CostumeId);

                    //CSO Entry
                    CSO_Entry csoEntry = new CSO_Entry();

                    csoEntry.CharaID = chara.CmsEntry.ID;
                    csoEntry.Costume = (uint)costume.CostumeId;
                    csoEntry.SePath = (se != null) ? Path.GetFileNameWithoutExtension(se.Path) : null;
                    csoEntry.VoxPath = (vox != null) ? Path.GetFileNameWithoutExtension(vox.Path) : null;
                    csoEntry.AmkPath = (amk != null) ? string.Format("{0}/{1}", Path.GetFileName(Path.GetDirectoryName(amk.Path)), Path.GetFileNameWithoutExtension(amk.Path)) : null;
                    csoEntry.SkillCharaCode = costume.CsoSkills;

                    csoEntries.Add(csoEntry);
                }
            }


            //Install CSO Entries
            csoFile.CsoEntries.RemoveAll(x => x.CharaID == chara.CmsEntry.ID);
            csoFile.CsoEntries.AddRange(csoEntries);
        }

        //Skill
        public ushort InstallPupEntries(IList<PUP_Entry> entries, int skillId1 = -1) 
        {
            if (entries?.Count == 0) return ushort.MaxValue;

            //First check if sequence already exists, and return the ID if it does
            int id = pupFile.CheckForSequence(entries);

            //Check if old IDs are used by any other skill, and reuse them if not (If prev check failed)
            if(entries[0].ID != -1 && skillId1 != -1 && id == -1)
            {
                if(!cusFile.IsPupEntryUsed(entries[0].ID, entries.Count, skillId1))
                {
                    id = entries[0].ID;
                }
            }

            if(id == -1)
            {
                id = pupFile.GetNewPupId(entries.Count);
            }

            if (id != -1)
            {
                PUP_File.SetPupId(entries, id);
                pupFile.PupEntries.AddRange(entries);
                return (ushort)id;
            }
            else
            {
                throw new InvalidOperationException("Xenoverse2:InstallPupEntries: pupId was -1, could not assign a new ID.");
            }

        }

        public void InstallSkillCusEntry(Skill cusEntry, CUS_File.SkillType skillType)
        {
            IList<Skill> skills = cusFile.GetSkills(skillType);

            int existingIdx = skills.IndexOf(skills.FirstOrDefault(x => x.ID2 == cusEntry.ID2));

            if(existingIdx != -1)
            {
                skills[existingIdx] = cusEntry;
            }
            else
            {
                skills.Add(cusEntry);
            }
        }

        public void InstallSkillIdbEntry(IDB_Entry idbEntry)
        {
            var existingIdx = skillIdbFile.Entries.IndexOf(skillIdbFile.Entries.FirstOrDefault(x => x.ID_Binding == idbEntry.ID_Binding && x.Type == idbEntry.Type));

            if(existingIdx != -1)
            {
                skillIdbFile.Entries[existingIdx] = idbEntry;
            }
            else
            {
                skillIdbFile.Entries.Add(idbEntry);
            }
        }

        public void InstallSkillName(string[] names, int id2, SkillType skillType)
        {
            if (skillType == SkillType.Blast) return;
            if (names.Length != (int)Language.NumLanguages) throw new InvalidDataException($"Xenoverse2.InstallSkillName: Invalid number of language entries.");

            MSG_File[] msgFiles = GetSkillNameMsgFile(skillType);

            for(int i = 0; i < msgFiles.Length; i++)
            {
                msgFiles[i].SetSkillName(names[i], id2, skillType);
            }
        }

        public void InstallSkillDesc(string[] desc, int id2, SkillType skillType)
        {
            if (skillType == SkillType.Blast) return;
            if (desc.Length != (int)Language.NumLanguages) throw new InvalidDataException($"Xenoverse2.InstallSkillDesc: Invalid number of language entries.");

            MSG_File[] msgFiles = GetSkillDescMsgFile(skillType);

            for (int i = 0; i < msgFiles.Length; i++)
            {
                msgFiles[i].SetSkillDesc(desc[i], id2, skillType);
            }
        }

        public void InstallSkillAwokenName(ObservableCollection<string[]> names, int id2)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Length != (int)Language.NumLanguages) throw new InvalidDataException($"Xenoverse2.InstallSkillAwokenName: Invalid number of language entries.");

                for (int a = 0; a < btlHudMsgFile.Length; a++)
                {
                    btlHudMsgFile[a].SetAwokenStageName(names[i][a], id2, i);
                }
            }
        }
        
        //Moveset/Character
        public void InstallErsCharacterEntry(string charaShortName, int id, bool hasEepk, bool borrowed)
        {
            var ersEntries = ersFile.GetSubentryList(2);

            var entry = ersEntries.FirstOrDefault(x => x.ID == id);
            string path = $"chara/{charaShortName}/{charaShortName}.eepk";

            if (hasEepk && !borrowed)
            {
                if (entry != null)
                {
                    entry.FILE_PATH = path;
                }
                else
                {
                    ersEntries.Add(new ERS_MainTableEntry() { FILE_PATH = path, ID = id });
                }
            }
            else if(!hasEepk && entry != null)
            {
                //Remove entry
                ersEntries.Remove(entry);
            }
        }

        public void InstallCmsEntry(CMS_Entry cmsEntry)
        {
            var existingIdx = cmsFile.CMS_Entries.IndexOf(cmsFile.CMS_Entries.FirstOrDefault(x => x.ID == cmsEntry.ID));

            if (existingIdx != -1)
            {
                cmsFile.CMS_Entries[existingIdx] = cmsEntry;
            }
            else
            {
                cmsFile.CMS_Entries.Add(cmsEntry);
            }
        }

        public void InstallCharacterName(string[] names, string shortName)
        {
            if (names.Length != (int)Language.NumLanguages) throw new InvalidDataException($"Xenoverse2.InstallCharacterName: Invalid number of language entries.");

            for (int i = 0; i < charaNameMsgFile.Length; i++)
            {
                charaNameMsgFile[i].SetCharacterName(names[i], shortName);
            }
        }

        #endregion



        public static Language SystemLanguageToXv2()
        {
            switch (CultureInfo.InstalledUICulture.TwoLetterISOLanguageName)
            {
                case "en":
                    return Language.English;
                case "fr":
                    return Language.French;
                case "it":
                    return Language.Italian;
                case "de":
                    return Language.German;
                case "es":
                    return Language.Spanish1;
                case "ca":
                    return Language.Spanish2;
                case "pt":
                    return Language.Portuguese;
                case "pl":
                    return Language.Polish;
                case "ru":
                    return Language.Russian;
                case "tw":
                    return Language.Chinese1;
                case "zh":
                    return Language.Chinese2;
                case "kr":
                    return Language.Korean;
                default:
                    return Language.English;
            }
        }

    }

    public class Xv2Item
    {
        public int ID { get; private set; }
        public string Name { get; private set; }

        public Xv2Item(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }

}
