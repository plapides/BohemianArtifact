using System;
using System.Collections;

namespace BohemianArtifact
{
    public class SelectableObjectManager
    {
        private Hashtable objectTable;
        private uint count;
        private Queue unusedIDs; // when an object is removed, its ID goes back in the pool, ready to be reassigned

        public SelectableObjectManager()
        {
            objectTable = new Hashtable();
            unusedIDs = new Queue();
            count = 0;
        }

        public uint AddObject(SelectableObject obj)
        {
            uint objID;
            if (unusedIDs.Count == 0)
                objID = ++count;
            else
                objID = (uint)unusedIDs.Dequeue();

            obj.SetObjectId(objID);
            objectTable.Add(objID, obj);
            return objID;
        }

        public void RemoveObject(SelectableObject obj)
        {
            if (obj.Id > 0)
            {
                if (!unusedIDs.Contains(obj.Id))
                    unusedIDs.Enqueue(obj.Id);
                objectTable.Remove(obj.Id);
            }
        }

        public SelectableObject FindObject(uint key)
        {
            return (SelectableObject)objectTable[key];
        }
    }
}
