using System;

namespace Paramore.Brighter
{
    internal class NullHandlerFactory : IAmAHandlerFactory, IAmAHandlerFactoryAsync
    {
        IHandleRequests IAmAHandlerFactory.Create(Type handlerType)
        {
            return null;
        }

        public void Release(IHandleRequests handler)
        {
        }

        IHandleRequestsAsync IAmAHandlerFactoryAsync.Create(Type handlerType)
        {
            return null;
        }

        public void Release(IHandleRequestsAsync handler)
        {
        }
    }
}