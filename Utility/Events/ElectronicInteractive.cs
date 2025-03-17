using System;
using UnityEngine;

namespace UB
{
    interface IElectronicObject
    {
        /// <summary>
        /// 활성화 시 호출할 함수입니다.
        /// </summary>
        Action CallActive { get; set; }
        /// <summary>
        /// 활성화 여부입니다. True일 경우에만 작동합니다.
        /// ex) 스위치 켜짐 Active = true
        /// ex) 적 죽음 Active = true
        /// </summary>
        bool Active { get; set; }
    }
}
