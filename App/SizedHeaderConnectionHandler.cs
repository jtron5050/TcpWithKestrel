using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using System.Buffers.Binary;
using System.Buffers;
using System;

namespace App
{
    internal class SizedHeaderConnectionHandler : ConnectionHandler
    {
        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            
            var input = connection.Transport.Input;

            while (true)
            {
                var readResult = await input.ReadAsync();
                var buffer = readResult.Buffer;

                if (TryReadMessage(ref buffer, out var message))
                {

                }

                if (readResult.IsCompleted)
                {
                    break;
                }

                input.AdvanceTo(buffer.Start, buffer.End);
            }
        }

        private bool TryReadMessage(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> message)
        {
            if (buffer.First.Span.Length < 4)
            {
                message = default(ReadOnlySequence<byte>);
                return false;
            }

            int messageLength = 0;
            if (buffer.Length >= 4)
            {
                messageLength = ParseMessageLength(buffer.First.Span);
            }

            if (buffer.Length < messageLength + sizeof(int))
            {
                message = default(ReadOnlySequence<byte>);
                return false;
            }

            message = buffer.Slice(sizeof(int), messageLength);
            return true;
        }

        private int ParseMessageLength(ReadOnlySpan<byte> buffer)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(buffer);        
        }
    }
}