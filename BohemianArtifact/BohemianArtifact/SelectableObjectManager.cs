using System;
using System.Collections;

namespace BohemianArtifact
{
    public class SelectableObjectManager
    {
        private Hashtable objectTable;
        private uint count;

        public SelectableObjectManager()
        {
            objectTable = new Hashtable();
            count = 0;
        }

        public uint AddObject(SelectableObject obj)
        {
            count += 1;
            obj.SetObjectId(count);
            objectTable.Add(count, obj);
            return count;
        }

        public SelectableObject FindObject(uint key)
        {
            return (SelectableObject)objectTable[key];
        }
    }
}
