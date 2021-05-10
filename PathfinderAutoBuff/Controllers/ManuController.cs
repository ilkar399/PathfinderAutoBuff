﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace PathfinderAutoBuff.Controllers
/*
 * Mod Menu UI controller
 * Based on Hsinyu Chan KingmakerModMaker
 * https://github.com/hsinyuhcan/KingmakerModMaker
 */
{
    public interface IMenuPage
    {
        string Name { get; }

        int Priority { get; }

        void OnGUI(UnityModManager.ModEntry modEntry);
    }

    public interface IMenuTopPage : IMenuPage { }

    public interface IMenuSelectablePage : IMenuPage { }

    public interface IMenuBottomPage : IMenuPage { }

    public class MenuController
    {
        #region Fields

        private int _tabIndex;
        private List<IMenuTopPage> _topPages = new List<IMenuTopPage>();
        private List<IMenuSelectablePage> _selectablePages = new List<IMenuSelectablePage>();
        private List<IMenuBottomPage> _bottomPages = new List<IMenuBottomPage>();

        #endregion

        #region Toggle

        public void Enable(UnityModManager.ModEntry modEntry, Assembly _assembly)
        {
            foreach (Type type in _assembly.GetTypes()
                .Where(type => !type.IsInterface && !type.IsAbstract && typeof(IMenuPage).IsAssignableFrom(type)))
            {
                if (typeof(IMenuTopPage).IsAssignableFrom(type))
                    _topPages.Add(Activator.CreateInstance(type, true) as IMenuTopPage);

                if (typeof(IMenuSelectablePage).IsAssignableFrom(type))
                    _selectablePages.Add(Activator.CreateInstance(type, true) as IMenuSelectablePage);

                if (typeof(IMenuBottomPage).IsAssignableFrom(type))
                    _bottomPages.Add(Activator.CreateInstance(type, true) as IMenuBottomPage);
            }

            int comparison(IMenuPage x, IMenuPage y) => x.Priority - y.Priority;
            _topPages.Sort(comparison);
            _selectablePages.Sort(comparison);
            _bottomPages.Sort(comparison);

            modEntry.OnGUI += OnGUI;
        }

        public void Disable(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI -= OnGUI;

            _topPages.Clear();
            _selectablePages.Clear();
            _bottomPages.Clear();
        }

        #endregion

        private void OnGUI(UnityModManager.ModEntry modEntry)
        {
            bool hasPriorPage = false;

            if (_topPages.Count > 0)
            {
                foreach (IMenuTopPage page in _topPages)
                {
                    if (hasPriorPage) GUILayout.Space(10f);
                    page.OnGUI(modEntry);
                    hasPriorPage = true;
                }
            }

            if (_selectablePages.Count > 0)
            {
                if (_selectablePages.Count > 1)
                {
                    if (hasPriorPage) GUILayout.Space(10f);
                    _tabIndex = GUILayout.Toolbar(_tabIndex, _selectablePages.Select(page => page.Name).ToArray());
                    GUILayout.Space(10f);
                }
                _selectablePages[_tabIndex].OnGUI(modEntry);
                hasPriorPage = true;
            }

            if (_bottomPages.Count > 0)
            {
                foreach (IMenuBottomPage page in _bottomPages)
                {
                    if (hasPriorPage) GUILayout.Space(10f);
                    page.OnGUI(modEntry);
                    hasPriorPage = true;
                }
            }
        }
    }
}
