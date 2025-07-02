
//namespace RestaurantManagement.Api.BackgroundTask
//{
//    public class BackgroundTaskUpdate : BackgroundService
//    {
//        protected override Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while(!stoppingToken.IsCancellationRequested)
//            {
//                // Your background task logic here
//                // For example, you can call a service method to update data periodically
//                var dataTable = new DataTable();
                
//                // Simulate work by delaying for a certain period
//                var now = DateTime.Now.AddMinutes(-15);
//                var res = DateTime.Now;
//                res<now


//                Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).Wait(stoppingToken);
//            }
//        }
//    }
//}
