using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Equipables
{
    // Alows equipable items to be mapped to an id and registration, for mods maybe?
    // yes I'm very sure of what i'm doin! :>
    public static class EquipablesDB
    {
        private static Dictionary<int, Type> EquipableMapping = new Dictionary<int, Type>();

        public static void AddEquipable<T>(short ID) where T : EquipableBase
        {
            AddEquipable(ID, typeof(T));
        }

        public static void AddEquipable(int ID, Type Type)
        {
            if (EquipableMapping.ContainsKey(ID)) { UnityEngine.Debug.Log($"Equipables: Tried to add equipable with ID {ID} but an equipable with that id has already been added."); }
            else
            {
                EquipableMapping.Add(ID, Type);
            }
        }

        public static EquipableBase CreateFromID(int ID)
        {
            if (EquipableMapping.TryGetValue(ID, out Type eType))
            {
                var equipable = Activator.CreateInstance(eType) as EquipableBase;
                return equipable;
            }
            else { UnityEngine.Debug.Log($"Equipables: Can't create equipable for ID: {ID}, no type mappng for that ID :<"); return null; } 
        }

        // Scan the loaded assemblys for EquipableBase sub classs and add them to the mapping
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ScanAndAddEquipables()
        {
            var ass = Assembly.GetExecutingAssembly();
            foreach (Type type in ass.GetTypes().Where(x => x.IsSubclassOf(typeof(EquipableBase))))
            {
                var prop = type.GetProperty("ItemID", BindingFlags.Public | BindingFlags.Static);
                var id   = (int)prop.GetValue(null, null);
                AddEquipable(id, type);
            }
        }
    }
}
