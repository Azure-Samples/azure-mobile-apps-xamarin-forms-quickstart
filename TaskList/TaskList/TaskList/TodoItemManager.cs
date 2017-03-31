/*
 * To add Offline Sync Support
 * 1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 * 2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see http://go.microsoft.com/fwlink/?LinkId=620342
 */
// #define OFFLINE_SYNC_ENABLED

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
#endif

namespace TaskList
{
    public class TodoItemManager
    {
        private static TodoItemManager defaultInstance = null;
        private MobileServiceClient client;

#if OFFLINE_SYNC_ENABLED
        IMobileServiceSyncTable<TodoItem> todoTable;

        const string offlineDbPath = @"localstore.db";
#else
        IMobileServiceTable<TodoItem> todoTable;
#endif

        private TodoItemManager()
        {
            client = new MobileServiceClient(Constants.ApplicationURL);

#if OFFLINE_SYNC_ENABLED
            var store = new MobileServiceSQLiteStore(offlineDbPath);

            // Define the tables stored in the offline cache
            store.DefineTable<TodoItem>();

            // Initialize the sync context
            client.SyncContext.InitializeAsync(store);

            // Get a reference to the sync table
            todoTable = client.GetSyncTable<TodoItem>();
#else
            // Get a reference to the online table
            todoTable = client.GetTable<TodoItem>();
#endif
        }

        public static TodoItemManager DefaultManager
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new TodoItemManager();
                }
                return defaultInstance;
            }
        }

        public MobileServiceClient CurrentClient
        {
            get { return client; }
        }

        public bool IsOfflineEnabled
        {
            get { return todoTable is IMobileServiceSyncTable<TodoItem>; }
        }

        public async Task<ObservableCollection<TodoItem>> GetTodoItemsAsync(bool syncItems = false)
        {
            try
            {
#if OFFLINE_SYNC_ENABLED
                if (syncItems)
                {
                    await SyncAsync();
                }
#endif

                IEnumerable<TodoItem> items = await todoTable
                    .Where(item => !item.Done)
                    .ToEnumerableAsync();

                return new ObservableCollection<TodoItem>(items);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine($"Invalid sync operation: {msioe.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sync Error: {ex.Message}");
            }
            return null;
        }

        public async Task SaveTaskAsync(TodoItem item)
        {
            if (item.Id == null)
            {
                await todoTable.InsertAsync(item);
            }
            else
            {
                await todoTable.UpdateAsync(item);
            }
        }

#if OFFLINE_SYNC_ENABLED
        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                await client.SyncContext.PushAsync();
                // The first paramter is a query name, used to implement incremental sync
                await todoTable.PullAsync("allTodoItems", todoTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple conflict handling.  A real application would handle the various errors like network
            // conditions, server conflicts and others via the IMobileServiceSyncHandler.  This version will
            // revert to the server copy or discard the local change (i.e. server wins policy)
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        // Revert to the server copy
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard the local change
                        await error.CancelAndDiscardItemAsync();
                    }
                    Debug.WriteLine($"Error executing sync operation on table {error.TableName}: {error.Item["id"]} (Operation Discarded)");
                }
            }
        }
#endif
    }
}
