using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class SimpleStackTransfer<T> : ITransfer<Quantity,T> where T : IValueAndID<Quantity>
    {
        readonly IInsert<T> _destination;
        readonly IExtract<Quantity, T> _source;

        public SimpleStackTransfer(IExtract<Quantity, T> source, IInsert<T> destination)
        {
            _source = source;
            _destination = destination;
        }

        public bool CanTransfer(Quantity arg)
        {
            return _destination.CanInsertAny(_source.Peek(arg));
        }

        public T TransferPossible(Quantity arg) => TransferUtilities.Transfer(_source, _destination, arg);
    }
}