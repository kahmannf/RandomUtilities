using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Queue
{
    public class FILO<T> : IQueue<T>
    {
        public bool IsThreadsafe => true;

        public bool HasItems => _lastItem != null;

        public T[] Clear()
        {
            QueueAccessResult<T> result = PerformOperation(new QueueAccessCommand<T>(QueueAccesTypes.Clear));

            if (!result.WasSuccessful)
            {
                throw GetExceptionFromResult(result);
            }
            else
            {
                return result.Items;
            }
        }

        public int Count()
        {
            return Count(x => true);
        }

        public int Count(Predicate<T> p)
        {
            if (HasItems)
            {
                QueueItem<T> current = _lastItem;

                int count = 0;

                while (current != null)
                {
                    if (p(current.Content))
                    {
                        count++;
                    }

                    current = current.RelatedItem;
                }

                return count;
            }
            else
            {
                return 0;
            }
        }

        public T Peek()
        {
            QueueAccessResult<T> result = PerformOperation(new QueueAccessCommand<T>(QueueAccesTypes.Peek));

            if (!result.WasSuccessful)
            {
                throw GetExceptionFromResult(result);
            }
            else
            {
                return result.Item;
            }
        }
        
        public T[] PeekAll()
        {
            QueueAccessResult<T> result = PerformOperation(new QueueAccessCommand<T>(QueueAccesTypes.PeekAll));
            
            if (!result.WasSuccessful)
            {
                throw GetExceptionFromResult(result);
            }
            else
            {
                return result.Items;
            }
        }

        public T Pull()
        {
            QueueAccessResult<T> result = PerformOperation(new QueueAccessCommand<T>(QueueAccesTypes.Pull));

            if (!result.WasSuccessful)
            {
                throw GetExceptionFromResult(result);
            }
            else
            {
                return result.Item;
            }
        }

        public void Push(T item)
        {
            QueueAccessResult<T> result = PerformOperation(new QueueAccessCommand<T>(item));

            if (!result.WasSuccessful)
            {
                throw GetExceptionFromResult(result);
            }
        }

        private QueueItem<T> _firstItem;
        private QueueItem<T> _lastItem;

        private bool _currentlyAccessed = false;
        public bool CurrentlyAccessed => _currentlyAccessed;
        private QueueAccesTypes _currentAccessType;

        private Exception GetExceptionFromResult(QueueAccessResult<T> result)
        {
            if (result.Exception != null && !string.IsNullOrEmpty(result.FailReason))
            {
                return new Exception(result.FailReason, result.Exception);
            }
            else if (result.Exception != null)
            {
                return new Exception("Error while accessing the queue. See InnerExceptions for details", result.Exception);
            }
            else if (!string.IsNullOrEmpty(result.FailReason))
            {
                return new Exception(result.FailReason);
            }
            else
            {
                return new Exception("Something went hooribly wrong inside the Queue. Cannot analyse the problem... :(");
            }
        }

        private QueueAccessResult<T> PerformOperation(QueueAccessCommand<T> command)
        {
            QueueAccessResult<T> result = new QueueAccessResult<T>()
            {
                WasSuccessful = false,
                FailReason = "None",
                Type = command.Type
            };

            if (command == null)
            {
                result.FailReason = "command was null";
                result.Exception = new ArgumentNullException("command");
            }
            else
            {
                object threadLock = new object();
                lock (threadLock)
                {
                    try
                    {
                        _currentlyAccessed = true;
                        _currentAccessType = command.Type;
                        switch (command.Type)
                        {
                            case QueueAccesTypes.Push:
                                #region Push

                                QueueItem<T> newQItem = new QueueItem<T>(command.Item);

                                if (_firstItem == null)
                                {
                                    _firstItem = _lastItem = newQItem;
                                }
                                else
                                {
                                    _firstItem.RelatedItem = newQItem;
                                    _firstItem = newQItem;
                                }
                                result.WasSuccessful = true;
                                break;

                                #endregion
                            case QueueAccesTypes.Pull:
                                #region Pull

                                if (!HasItems)
                                {
                                    result.Exception = new InvalidOperationException("Tried to pull from an empty queue");
                                    result.FailReason = "There was no item inside the queue";
                                }
                                else
                                {
                                    result.Item = PullInternal();

                                    result.WasSuccessful = true;
                                }
                                break;

                                #endregion
                            case QueueAccesTypes.Peek:
                                #region Peek

                                if (!HasItems)
                                {
                                    result.Exception = new InvalidOperationException("Tried to peek into an empty queue");
                                    result.FailReason = "There was no item inside the queue";
                                }
                                else
                                {
                                    result.Item = _lastItem.Content;
                                    result.WasSuccessful = true;
                                }
                                break;

                                #endregion
                            case QueueAccesTypes.Clear:
                                #region PeekAll

                                List<T> allItems = new List<T>();
                                
                                QueueItem<T> currentItem = _lastItem;
                                
                                while(currentItem != null)
                                {
                                    allItems.Add(currentItem.Content);
                                    currentItem = currentItem.RelatedItem;
                                }
                                
                                result.Items = allItems.ToArray();

                                result.WasSuccessful = true;

                                break;

                                #endregion
                            case QueueAccesTypes.Clear:
                                #region Clear

                                List<T> allItems = new List<T>();

                                while (HasItems)
                                {
                                    allItems.Add(PullInternal());
                                }
                                
                                result.Items = allItems.ToArray();

                                result.WasSuccessful = true;

                                break;

                            #endregion
                            default:
                                result.FailReason = "Invalid Value for \"QueueAccessCommand<T>.Type\"";
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Exception = ex;
                        result.FailReason = "An unexpected Exception occured during Queue-Access.";
                        result.WasSuccessful = false;
                    }
                }
            }

            return result;
        }

        #region QueueAcces Methodes. Only call from inside the queue threadlock

        private T PullInternal()
        {
            T rv = _lastItem.Content;

            if (_lastItem == _firstItem)
            {
                _lastItem = _firstItem = null;
            }
            else
            {
                _lastItem = _lastItem.RelatedItem;
            }

            return rv;
        }

        #endregion
    }
}
