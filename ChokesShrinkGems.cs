using BepInEx;
using BiendeoCHLib;
using BiendeoCHLib.Patches;
using BiendeoCHLib.Patches.Attributes;
using BiendeoCHLib.Wrappers;
using BiendeoCHLib.Wrappers.Attributes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GUI;
using Rewired;

namespace ChokesShrinkGems
{
    [HarmonyCHPatch(typeof(BasePlayerWrapper), nameof(BasePlayerWrapper.MissNote))]
    public class MissNoteHandler
    {
        [HarmonyCHPostfix]
        static void Postfix(object __0)
        {
            var note = NoteWrapper.Wrap(__0);
            ChokesShrinkGems.Instance.MissNotePostfix(note);
        }
    }

    [BepInPlugin("com.yoshiog.clongemsizechoke", "ChokesShrinkGems", "1.0.1")]
    [BepInDependency("com.biendeo.biendeochlib")]
    public class ChokesShrinkGems : BaseUnityPlugin
    {
        /* // unnecessary stuff, might delete later
        public class ThingsHiddenFromStreamWhenWritingCode
        {
            public static string scoreDisableThing = "[REDACTED]";
        } 
        // */
        internal void MissNotePostfix(NoteWrapper note)
        {
            gemMult -= 1f;
            if (gameMgr.BasePlayers[0].IsSPActive)
            {
                gemMult += 0.5f;
            }
            if (gemMult < 0f)
            {
                gemMult = 0f;
            }
            SetGemSize(gemMult);
        }
        public static ChokesShrinkGems Instance { get; private set; }
        private Harmony Harmony;
        public ChokesShrinkGems()
        {
            Instance = this;
            Harmony = new Harmony("com.yoshiog.clongemsizechoke");
            PatchBase.InitializePatches(Harmony, Assembly.GetExecutingAssembly(), Logger);
        }
        public GameManager gmObj;
        public GameManagerWrapper gameMgr;
        public float defGemSize;
        public float gemMult;
        public string sceneName;
        public bool sceneChanged;
        // private FieldInfo scoreDisableField;

        public void SetGemSize(float percent = 100f)
        {
            foreach (var gameObj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (gameObj.name.StartsWith("Note"))
                {
                    gameObj.transform.localScale = new Vector3(defGemSize * percent / 100, defGemSize * percent / 100, 1f);
                }
                if (gameObj.name.StartsWith("Sustain(Clone)"))
                {
                    gameObj.transform.localScale = new Vector3(percent / 100, 1f, 1f);
                }
            }
        }
        public void Dbg_SetGemSizeToQuarter()
        {
            gemMult = 25f;
            SetGemSize(25f);
        }
        public void Dbg_ResetGemSize()
        {
            gemMult = 100f;
            SetGemSize();
        }
        #region Unity Methods
        public void Start()
        {
            SceneManager.activeSceneChanged += delegate (Scene _, Scene __)
            {
                sceneChanged = true;
            };
            // scoreDisableField = typeof(GlobalVariables).GetField(ThingsHiddenFromStreamWhenWritingCode.scoreDisableThing);
            defGemSize = 1.1f;
            gemMult = 100f;
            sceneName = "";
        }
        public void LateUpdate()
        {
            sceneName = SceneManager.GetActiveScene().name;
            if (this.sceneChanged && gameMgr.IsNull())
            {
                gmObj = GameObject.Find("Game Manager").GetComponent<GameManager>();
                gameMgr = GameManagerWrapper.Wrap(gmObj);
                defGemSize = 1.1f * ((float)gameMgr.GlobalVariables.GameGemSize.CurrentValue / 100);
                SetGemSize();
                gemMult = 100f;
                // // Disable score saving because reasons.  Might not be necessary.
                // scoreDisableField.SetValue(gameMgr.GlobalVariables.GlobalVariables, true);
                this.sceneChanged = false;
            }
        }
        #endregion
    }
}
