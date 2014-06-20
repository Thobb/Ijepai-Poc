using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Thobb.Ijepai.ConfigManager;

namespace Thobb.Ijepai.Messaging
{
    public sealed class AzureMessageQueueProvider
    {

        private CloudStorageAccount _storageAccount;
        private CloudQueueClient _queueStorage;
        private CloudQueue _queue = null;

        public AzureMessageQueueProvider()
        {
            Initialise(ConfigManager.ConfigManager.DefaultQueueName);
        }

        public AzureMessageQueueProvider(string queueName)
        {
            Initialise(queueName);
        }

        public void Initialise(string queueName)
        {

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("QueueName cannot be null or empty", "queueName");
            }

            string connStr = ConfigManager.ConfigManager.GetConfigurationSetting(ConfigManager.ConfigManager.DefaultQueueStorageConnString, string.Empty);
            _storageAccount = CloudStorageAccount.Parse(connStr);

            _queueStorage = _storageAccount.CreateCloudQueueClient();

            _queue = _queueStorage.GetQueueReference(queueName);

            _queue.CreateIfNotExists();

        }

        public CloudQueue ActiveQueue
        {
            get
            {
                return _queue;
            }
        }

        public void Send(string message)
        {
            _queue.AddMessage(new CloudQueueMessage(System.Text.Encoding.UTF8.GetBytes(message)));
        }

        public void Send(CloudQueueMessage message)
        {
            _queue.AddMessage(message);
        }

        public CloudQueueMessage Receive()
        {
            CloudQueueMessage message = _queue.GetMessage();
            if (message != null)
            {
                _queue.DeleteMessage(message);
            }
            return message;
        }

        public CloudQueueMessage Peek()
        {
            CloudQueueMessage message = _queue.GetMessage();
            return message;
        }

        public void Remove(CloudQueueMessage message)
        {
            if (message != null)
            {
                _queue.DeleteMessage(message);
            }
        }

    }
}
