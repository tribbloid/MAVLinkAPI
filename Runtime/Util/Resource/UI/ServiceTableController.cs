#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofill;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.Util.Resource.UI
{
    public class ServiceTableController : LifetimeBinding
    {
        [Autofill] public TableLayout table = null!;
        [Required] public ServiceRowController rowTemplate = null!;

        private readonly Queue<Cleanable> _pending = new();

        // Helper class to manage lifetime and row creation
        private class LifetimeWithSync : Lifetime
        {
            private readonly ServiceTableController _outer;

            public LifetimeWithSync(ServiceTableController table) // Calls Lifetime(IntPtr.Zero, true)
            {
                _outer = table;
            }

            public override void Register(Cleanable cleanable)
            {
                base.Register(cleanable);
                _outer._pending.Enqueue(cleanable);

                // // Instantiate the new row using the controller's context
                // var row = Instantiate(_outer.rowTemplate);
                // _outer.table.AddRow(row.row);
                // row.gameObject.SetActive(true);
                //
                // // TODO: the cleanable is still largely not initialized, so it is illegal
                // //  update should happen in next frame
                // row.Bind(cleanable);
            }
        }

        public override Lifetime Lifetime => lifetimeExisting.Lazy(() => new LifetimeWithSync(this))!;

        public void AddDummy()
        {
            _ = new Cleanable.Dummy(Lifetime);
        }

        public void AddDummy_Fail()
        {
            _ = new Cleanable.Dummy(Lifetime);

            var items = new List<int> { 1, 1, 2, 2 };

            try
            {
                items.Retry().FixedInterval.Run((i, elapsed) =>
                    throw new Exception($"Failure on item {i}"));
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                // Debug.LogException(e);
                throw;
            }
        }

        // public void Start()
        // {
        //     rowTemplate.gameObject.SetActive(false);
        // }

        public void Update()
        {
            while (_pending.Count > 0)
            {
                var cleanable = _pending.Dequeue();
                var row = Instantiate(rowTemplate);
                table.AddRow(row.row);
                row.gameObject.SetActive(true);
                row.Bind(cleanable);
            }
        }
    }
}