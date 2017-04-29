
#region ================== Copyright (c) 2016 Guillaume Levieux
/*
 * Copyright (c) 2016 Guillaume Levieux
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using System.Drawing;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;

#endregion

namespace GenerativeDoom
{
    //
    // MANDATORY: The plug!
    // This is an important class to the Doom Builder core. Every plugin must
    // have exactly 1 class that inherits from Plug. When the plugin is loaded,
    // this class is instantiated and used to receive events from the core.
    // Make sure the class is public, because only public classes can be seen
    // by the core.
    //

    public class BuilderPlug : Plug
    {
        // Static instance. We can't use a real static class, because BuilderPlug must
        // be instantiated by the core, so we keep a static reference. (this technique
        // should be familiar to object-oriented programmers)
        private static BuilderPlug me;

        // Static property to access the BuilderPlug
        public static BuilderPlug Me { get { return me; } }

        // We keep the statistics window loaded so that it retains its position and settings
        // and everything even if the statistics mode is not active.
        private GDForm gdform;

        // I don't like publicly visible members in a class, so I make this property to access
        // this form from the statistics editing mode.
        public GDForm GDForm { get { return gdform; } }

        // Override this property if you want to give your plugin a name other
        // than the filename without extention.
        public override string Name { get { return "Generative Doom Plugin"; } }

        // This event is called when the plugin is initialized
        public override void OnInitialize()
        {
            base.OnInitialize();

            // Keep a static reference
            me = this;

            // Load our statistics form
            gdform = new GDForm();
        }

        // This is called when the plugin is terminated
        public override void Dispose()
        {
            base.Dispose();

            // Time to clean everything up
            gdform.Dispose();
            gdform = null;
        }
    }
}
