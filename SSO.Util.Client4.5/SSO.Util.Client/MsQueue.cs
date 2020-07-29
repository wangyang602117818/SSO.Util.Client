using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 微软messagequeue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MsQueue<T>
    {
        private string path = "";
        private string managerpath = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">FormatName:DIRECT=OS:computename\\private$\\task_queue</param>
        /// <param name="managerpath"></param>
        public MsQueue(string path, string managerpath = "")
        {
            this.path = path;
            this.managerpath = managerpath;
        }
        /// <summary>
        /// 不能操作远程队列,只能由本地的程序创建队列,程序只能对远程的队列发送消息
        /// 创建队列需要消耗较多资源,确保只创建一次
        /// </summary>
        /// <param name="transactional">是否是事务队列</param>
        public void CreateQueue(bool transactional = false)
        {
            try
            {
                if (!MessageQueue.Exists(path)) MessageQueue.Create(path, transactional).SetPermissions("Everyone", MessageQueueAccessRights.FullControl);
                if (!string.IsNullOrEmpty(managerpath) && !MessageQueue.Exists(managerpath)) MessageQueue.Create(managerpath, transactional).SetPermissions("Everyone", MessageQueueAccessRights.FullControl);
            }
            catch (Exception ex)
            {
                Log4Net.ErrorLog(ex);
            }
        }
        /// <summary>
        /// 发送普通消息
        /// </summary>
        /// <param name="data">消息体</param>
        /// <param name="label">消息标签</param>
        /// <param name="recoverable">消息持久化</param>
        public void SendMessage(T data, string label, bool recoverable = false)
        {
            MessageQueue messageQueue = new MessageQueue(path);
            Message message = new Message(data)
            {
                Recoverable = recoverable,
            };
            messageQueue.Send(message, label);
        }
        /// <summary>
        /// 发送需要应答的消息
        /// </summary>
        /// <param name="data">消息体</param>
        /// <param name="label">消息标签</param>
        /// <param name="recoverable">消息持久化</param>
        public void SendMessageAck(T data, string label, bool recoverable = false)
        {
            MessageQueue messageQueue = new MessageQueue(path);
            Message message = new Message(data) { Recoverable = recoverable };
            message.AdministrationQueue = new MessageQueue(managerpath);
            message.AcknowledgeType = AcknowledgeTypes.PositiveReceive | AcknowledgeTypes.PositiveArrival;
            messageQueue.Send(message, label);
        }
        /// <summary>
        /// 事务性队列只能发送事务性消息,发送普通消息会丢弃
        /// </summary>
        /// <param name="data"></param>
        /// <param name="label"></param>
        public void SendMessageTransactional(T data, string label, bool recoverable = false)
        {
            MessageQueue messageQueue = new MessageQueue(path);
            MessageQueueTransaction myTransaction = new MessageQueueTransaction();
            myTransaction.Begin();
            Message message = new Message(data) { Recoverable = recoverable };
            messageQueue.Send(message, label, myTransaction);
            myTransaction.Commit();
        }
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="action"></param>
        public void ReceiveMessage(Action<T> action)
        {
            MessageQueue messageQueue = new MessageQueue(path);
            messageQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
            while (true)
            {
                var obj = messageQueue.Receive();
                T t = (T)obj.Body;
                action(t);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void ReceiveDeadletterMessage(Action<T> action)
        {
            MessageQueue deadLetter = new MessageQueue(".\\DeadLetter$");
            deadLetter.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
            while (true)
            {
                var obj = deadLetter.Receive();
                T t = (T)obj.Body;
                action(t);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        public void ReceiveMessageAck(Func<T, bool> func)
        {
            MessageQueue messageQueue = new MessageQueue(path);
            messageQueue.MessageReadPropertyFilter.CorrelationId = true;
            messageQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
            while (true)
            {
                var myMessage = messageQueue.Receive();
                T t = (T)myMessage.Body;
                //处理消息.如果返回true
                if (func(t))
                {
                    ReceiveAcknowledgment(myMessage.Id, managerpath);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        public void ReceiveMessageTransactional(Func<T, bool> func)
        {
            MessageQueue messageQueue = new MessageQueue(path);
            messageQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
            while (true)
            {
                MessageQueueTransaction myTransaction = new MessageQueueTransaction();
                try
                {
                    myTransaction.Begin();
                    var obj = messageQueue.Receive(myTransaction);
                    T t = (T)obj.Body;
                    if (func(t))
                    {
                        myTransaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    myTransaction.Abort();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="queuePath"></param>
        public void ReceiveAcknowledgment(string messageId, string queuePath)
        {
            bool found = false;
            MessageQueue queue = new MessageQueue(queuePath);
            queue.MessageReadPropertyFilter.CorrelationId = true;
            queue.MessageReadPropertyFilter.Acknowledgment = true;
            try
            {
                while (queue.PeekByCorrelationId(messageId) != null)
                {
                    Message myAcknowledgmentMessage = queue.ReceiveByCorrelationId(messageId);

                    // Output acknowledgment message information. The correlation Id is identical
                    // to the id of the original message.
                    Console.WriteLine("Acknowledgment Message Information--");
                    Console.WriteLine("Correlation Id: " + myAcknowledgmentMessage.CorrelationId.ToString());
                    Console.WriteLine("Id: " + myAcknowledgmentMessage.Id.ToString());
                    Console.WriteLine("Acknowledgment Type: " + myAcknowledgmentMessage.Acknowledgment.ToString());
                    Console.WriteLine("____________________________________________");

                    found = true;
                }
            }
            catch (InvalidOperationException e)
            {
                // This exception would be thrown if there is no (further) acknowledgment message
                // with the specified correlation Id. Only output a message if there are no messages;
                // not if the loop has found at least one.
                if (found == false)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
