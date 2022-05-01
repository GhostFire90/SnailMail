using System.Threading.Tasks;
using System.Collections;

namespace SnailMailProtocol
{
    enum ServerCodes { ClientSend, ClientRecieve, ClientDisconnect, ClientRequestInbox }
    enum OperationCodes { WaitTimeOver, WaitTime, FileDoesntExist, FileDoesExist }
    enum HandshakeCodes { ServerHasKey, ServerDoesntHaveKey }

}
