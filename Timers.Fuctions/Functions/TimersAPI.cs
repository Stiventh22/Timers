using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Timers.Common.Models;
using Timers.Common.Responses;
using Timers.Fuctions.Entities;

namespace Timers.Fuctions.Functions
{
    public static class TimersAPI
    {
        [FunctionName(nameof(CreateTimers))]
        public static async Task<IActionResult> CreateTimers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "timers")] HttpRequest req,
            [Table("timers", Connection = "AzureWebJobsStorage")] CloudTable timersTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new Working Time Employees");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Timerss timerss = JsonConvert.DeserializeObject<Timerss>(requestBody);
            if (string.IsNullOrEmpty(timerss?.IdEmployee.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The Request must have a employee identification"
                });
            }
            TimersEntity timersEntity = new TimersEntity
            {
                IdEmployee = timerss.IdEmployee,
                WorkTime = DateTime.UtcNow,
                Type = timerss.Type,
                Consolidated = false,
                PartitionKey = "WORKTIME",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

            TableOperation addOperationWorkingTime = TableOperation.Insert(timersEntity);
            await timersTable.ExecuteAsync(addOperationWorkingTime);

            log.LogInformation("Add a new employee in table");

            return new OkObjectResult(new Response
            {
                IdEmployees = timersEntity.IdEmployee,
                Work_Time = timersEntity.WorkTime,
                Message = "The information has been successfully registered"
            });
        }


        [FunctionName(nameof(UpdatedTimers))]
        public static async Task<IActionResult> UpdatedTimers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "timers/{IdEmployee}")] HttpRequest req,
            [Table("timers", Connection = "AzureWebJobsStorage")] CloudTable timersTable,
            string IdEmployee,
            ILogger log)
        {
            log.LogInformation($"The employee record will be updated: {IdEmployee}, in the table WorkingTimeEmployees");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Timerss timerss = JsonConvert.DeserializeObject<Timerss>(requestBody);

            TableOperation findOperationTimerss = TableOperation.Retrieve<TimersEntity>("WORKTIME", IdEmployee);
            TableResult findResultWorkingTimerss = await timersTable.ExecuteAsync(findOperationTimerss);

            //Valid if the id employee was found successfully
            if (findResultWorkingTimerss.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = $"The employee identified with the identification number: {IdEmployee}, was not found."
                });
            }

            TimersEntity timersEntity = (TimersEntity)findResultWorkingTimerss.Result;
            timersEntity.WorkTime = timerss.WorkTime;
            timersEntity.Type = timerss.Type;

            //Validate the registration date and time
            if (string.IsNullOrEmpty(timerss.WorkTime.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The indicated request must comply with the following format in order to make the change, YYYY-MM-DD:HH:MM:SS",
                });
            }

            // Validate Type
            if (string.IsNullOrEmpty(timerss.Type.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The data entered is not valid in the Type field."
                });
            }


            TableOperation updateOperationWorkingTime = TableOperation.Replace(timersEntity);
            await timersTable.ExecuteAsync(updateOperationWorkingTime);

            log.LogInformation("Employed update in the table Working Time Employed");

            return new OkObjectResult(new Response
            {
                IdEmployees = timersEntity.IdEmployee,
                Message = "The information has been successfully updated"
            });
        }

        [FunctionName(nameof(GetAllTimers))]
        public static async Task<IActionResult> GetAllTimers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timers")] HttpRequest req,
            [Table("timers", Connection = "AzureWebJobsStorage")] CloudTable timersTable,
            ILogger log)
        {
            log.LogInformation("All jobs recieved.");

            TableQuery<TimersEntity> query = new TableQuery<TimersEntity>();
            TableQuerySegment<TimersEntity> workings = await timersTable.ExecuteQuerySegmentedAsync(query, null);

            log.LogInformation("Retrieved all workings");

            return new OkObjectResult(new Response
            {
                Message = "Retrieved all workings",
                Result = workings
            });
        }

        [FunctionName(nameof(GetAllTimersById))]
        public static IActionResult GetAllTimersById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timers/{IdEmployee}")] HttpRequest req,
            [Table("worktime", "WORKTIME", "{IdEmployee}", Connection = "AzureWebJobsStorage")] TimersEntity timersEntity,
            string Id_Employees,
            ILogger log)
        {
            log.LogInformation($"Get working by Id: {Id_Employees}, recieved.");

            if (timersEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Working not found."
                });
            }

            log.LogInformation($"Working: {timersEntity.RowKey}, retrieved.");

            return new OkObjectResult(new Response
            {
                Message = "Retrieved working",
                Result = timersEntity
            });
        }

        [FunctionName(nameof(DeleteTimers))]
        public static async Task<IActionResult> DeleteTimers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "WorkingTimeEmployees/{IdEmployee}")] HttpRequest req,
            [Table("worktime", "WORKTIME", "{IdEmployee}", Connection = "AzureWebJobsStorage")] TimersEntity timersEntity,
             [Table("worktime", Connection = "AzureWebJobsStorage")] CloudTable timersTable,
            string IdEmployee,
            ILogger log)
        {
            log.LogInformation($"Delete working: {IdEmployee}, recieved.");

            if (timersEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Working not found."
                });
            }

            await timersTable.ExecuteAsync(TableOperation.Delete(timersEntity));
            log.LogInformation($"Working: {timersEntity.RowKey}, deleted.");

            return new OkObjectResult(new Response
            {
                Message = "Deleted working",
                Result = timersEntity
            });
        }

    }
}

