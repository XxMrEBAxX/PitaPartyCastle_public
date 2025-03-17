using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace UB
{
    public class SoundManager : Singleton<SoundManager>
    {
        public enum SFXNames
        {
            None = -1,
            Jump,
            Land,
            Attack,
            Return,
            ReturnHit,
            Hit
        }
        public EventReference clipJump;
        public EventReference clipLand;
        public EventReference clipAttack;
        public EventReference clipReturn;
        public EventReference clipHit;
        public EventReference clipReturnHit;
        //private Dictionary<string, EventReference> _efxDictionary;
        private string[] _sfxNames =
        {
            "C_Jump",
            "C_Land",
            "CU_Atk",
            "CU_Return",
            "CU_ReturnHit",
            "E_Hit"
        };

        void Start()
        {
            // _efxDictionary = new Dictionary<string, EventReference>();
            // foreach (EditorEventRef e in EventManager.Events)
            // {
            //     string[] pathSplited = e.Path.Split('/');
            //     try
            //     {
            //         if (pathSplited[pathSplited.Length - 2] == "FX")
            //         {
            //             string eventName = pathSplited[pathSplited.Length - 1];
            //             var eventRef = EventReference.Find(e.Path);
            //             _efxDictionary.Add(eventName, eventRef);
            //         }
            //     }
            //     catch
            //     {
            //         continue;
            //     }
            // }
        }
        public void PlaySFX(SFXNames name)
        {
            //RuntimeManager.PlayOneShot(_efxDictionary[_sfxNames[(int)name]]);
            switch(name)
            {
                case SFXNames.Jump:
                    RuntimeManager.PlayOneShot(clipJump);
                    break;
                case SFXNames.Attack:
                    RuntimeManager.PlayOneShot(clipAttack);
                    break;
                case SFXNames.Hit:
                    RuntimeManager.PlayOneShot(clipHit);
                    break;
                case SFXNames.Return:
                    RuntimeManager.PlayOneShot(clipReturn);
                    break;
                case SFXNames.ReturnHit:
                    RuntimeManager.PlayOneShot(clipReturnHit);
                    break;
                case SFXNames.Land:
                    RuntimeManager.PlayOneShot(clipLand);
                    break;
            }
        }
    }

}