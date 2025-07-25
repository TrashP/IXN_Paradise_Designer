using System;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public static class TransferUtilities
    {
        public static T Transfer<T>(IExtract<Quantity, T> _source, IInsert<T> _destination, Quantity arg) where T : IValueAndID<Quantity>
        {
            var peeked = _source.Peek(arg);
            arg = Math.Min(peeked.Value, arg);
            var remainder = _destination.RemainderIfInserted(peeked);
            var extracted = _source.ExtractAmount(arg - remainder.Value);
            _destination.InsertCompletely(extracted);
            return extracted;
        }
    }
}