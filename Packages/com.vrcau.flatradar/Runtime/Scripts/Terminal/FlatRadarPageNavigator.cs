﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FlatRadarPageNavigator : UdonSharpBehaviour
    {
        public Toggle[] navItems = { };
        public GameObject[] pages = { };

        private void Start() {
            _InvokeNavigatorUpdate();
        }

        public void _InvokeNavigatorUpdate() {
            for (var index = 0; index < navItems.Length; index++) {
                var navItem = navItems[index];
                var page = pages[index];

                if (!navItem.isOn) continue;

                ResetAllPages();
                page.SetActive(true);

                return;
            }
        }

        private void ResetAllPages() {
            foreach (var page in pages)
                page.SetActive(false);
        }
    }
}