//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Xunit;
//using Ripple.EventManagement.Api.Controllers;

//namespace Ripple.EventManagement.Tests
//{
//    // Lightweight test sender that returns preconfigured responses based on the request runtime type name.
//    class TestSender : ISender
//    {
//        private readonly Dictionary<string, object?> _responses;
//        public TestSender(Dictionary<string, object?> responses) => _responses = responses;

//        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
//        {
//            var key = request.GetType().Name;
//            if (_responses.TryGetValue(key, out var resp))
//            {
//                return Task.FromResult((TResponse)resp!);
//            }
//            return Task.FromResult(default(TResponse)!);
//        }

//        IAsyncEnumerable<TResponse> ISender.CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        IAsyncEnumerable<object> ISender.CreateStream(object request, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        Task<TResponse> ISender.Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        Task ISender.Send<TRequest>(TRequest request, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        Task<object> ISender.Send(object request, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class EventsControllerGeneratedTests
//    {
//        private static Assembly LoadAppAssembly()
//        {
//            // assume assembly name matches project
//            return Assembly.Load("Ripple.EventManagement.Application");
//        }

//        private static object? CreateSampleEventDto(Assembly app)
//        {
//            var dtoType = app.GetTypes().FirstOrDefault(t => t.Name.EndsWith("EventDto"));
//            if (dtoType is null) return null;

//            // find a constructor and supply plausible values
//            var ctor = dtoType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
//            if (ctor is null) return Activator.CreateInstance(dtoType);

//            var args = ctor.GetParameters().Select(p => SampleValueForType(app, p.ParameterType)).ToArray();
//            return ctor.Invoke(args);
//        }

//        private static object? SampleValueForType(Assembly app, Type t)
//        {
//            if (t == typeof(Guid)) return Guid.NewGuid();
//            if (t == typeof(string)) return "sample";
//            if (t == typeof(int)) return 100;
//            if (t.Name == "DateOnly") return (object)DateOnly.FromDateTime(DateTime.UtcNow);
//            if (t.Name == "TimeOnly") return (object)TimeOnly.FromDateTime(DateTime.UtcNow);
//            if (t.IsArray)
//            {
//                var elem = SampleElementForArrayType(app, t.GetElementType()!);
//                var arr = Array.CreateInstance(t.GetElementType()!, 0);
//                return arr;
//            }

//            // for other reference types, try to create default instance
//            try { return Activator.CreateInstance(t); } catch { return null; }
//        }

//        private static object? SampleElementForArrayType(Assembly app, Type elemType)
//        {
//            if (elemType == typeof(string)) return "sample";
//            var pricingType = app.GetTypes().FirstOrDefault(t => t.Name.EndsWith("PricingTierDto"));
//            if (pricingType != null && elemType.Name == pricingType.Name)
//            {
//                // try to construct a pricing tier dto if possible
//                var ctor = pricingType.GetConstructors().FirstOrDefault();
//                if (ctor != null)
//                {
//                    var args = ctor.GetParameters().Select(p => SampleValueForType(app, p.ParameterType)).ToArray();
//                    return ctor.Invoke(args);
//                }
//                return Activator.CreateInstance(pricingType);
//            }
//            try { return Activator.CreateInstance(elemType); } catch { return null; }
//        }

//        [Fact]
//        public async Task GetAll_ReturnsOk()
//        {
//            var app = LoadAppAssembly();
//            var sampleDto = CreateSampleEventDto(app);
//            object? list = null;
//            if (sampleDto != null)
//            {
//                var dtoType = sampleDto.GetType();
//                var arr = Array.CreateInstance(dtoType, 1);
//                arr.SetValue(sampleDto, 0);
//                list = arr;
//            }

//            var responses = new Dictionary<string, object?>
//            {
//                ["GetEventsQuery"] = list
//            };

//            var controller = new EventsController(new TestSender(responses));
//            var result = await controller.Get(CancellationToken.None);
//            Assert.IsType<OkObjectResult>(result);
//        }

//        [Fact]
//        public async Task Get_ReturnsOk_WhenFound()
//        {
//            var app = LoadAppAssembly();
//            var sampleDto = CreateSampleEventDto(app);
//            var responses = new Dictionary<string, object?>
//            {
//                ["GetEventByIdQuery"] = sampleDto
//            };

//            var controller = new EventsController(new TestSender(responses));
//            var id = Guid.NewGuid();
//            var result = await controller.Get(CancellationToken.None);
//            Assert.IsType<OkObjectResult>(result);
//        }

//        [Fact]
//        public async Task Get_ReturnsNotFound_WhenMissing()
//        {
//            var responses = new Dictionary<string, object?> { ["GetEventByIdQuery"] = null };
//            var controller = new EventsController(new TestSender(responses));
//            var id = Guid.NewGuid();
//            var result = await controller.Get(CancellationToken.None);
//            // Accept both NotFoundResult and NotFoundObjectResult by checking status code 404
//            int? statusCode = null;
//            if (result is IActionResult actionResult)
//                statusCode = (actionResult as StatusCodeResult)?.StatusCode ?? (actionResult as ObjectResult)?.StatusCode;
//            else if (result is IConvertToActionResult convertToActionResult)
//                statusCode = (convertToActionResult.Convert() as StatusCodeResult)?.StatusCode ?? (convertToActionResult.Convert() as ObjectResult)?.StatusCode;
//            else
//                statusCode = null;

//            Assert.Equal(404, statusCode);
//        }

//        [Fact]
//        public async Task Create_ReturnsCreatedAt()
//        {
//            var responses = new Dictionary<string, object?> { ["CreateEventCommand"] = Guid.NewGuid() };
//            var controller = new EventsController(new TestSender(responses));

//            // construct a create command instance via reflection
//            var app = LoadAppAssembly();
//            var createType = app.GetTypes().FirstOrDefault(t => t.Name == "CreateEventCommand");
//            object? createCmd = null;
//            if (createType != null)
//            {
//                var ctor = createType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
//                if (ctor != null)
//                {
//                    var args = ctor.GetParameters().Select(p => SampleValueForType(app, p.ParameterType)).ToArray();
//                    createCmd = ctor.Invoke(args);
//                }
//            }

//            var result = await controller.Create((dynamic)createCmd!, CancellationToken.None);
//            Assert.IsType<CreatedAtActionResult>(result);
//        }

//        [Fact]
//        public async Task Update_ReturnsNoContent_WhenUpdated()
//        {
//            var responses = new Dictionary<string, object?> { ["UpdateEventCommand"] = true };
//            var controller = new EventsController(new TestSender(responses));

//            var app = LoadAppAssembly();
//            var updateType = app.GetTypes().FirstOrDefault(t => t.Name == "UpdateEventCommand");
//            object? updateCmd = null;
//            if (updateType != null)
//            {
//                var ctor = updateType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
//                if (ctor != null)
//                {
//                    var args = ctor.GetParameters().Select(p => SampleValueForType(app, p.ParameterType)).ToArray();
//                    updateCmd = ctor.Invoke(args);
//                }
//            }

//            var id = Guid.NewGuid();
//            var result = await controller.Update(id, (dynamic)updateCmd!, CancellationToken.None);
//            Assert.IsType<NoContentResult>(result);
//        }

//        [Fact]
//        public async Task Update_ReturnsNotFound_WhenMissing()
//        {
//            var responses = new Dictionary<string, object?> { ["UpdateEventCommand"] = false };
//            var controller = new EventsController(new TestSender(responses));

//            var app = LoadAppAssembly();
//            var updateType = app.GetTypes().FirstOrDefault(t => t.Name == "UpdateEventCommand");
//            object? updateCmd = null;
//            if (updateType != null)
//            {
//                var ctor = updateType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
//                if (ctor != null)
//                {
//                    var args = ctor.GetParameters().Select(p => SampleValueForType(app, p.ParameterType)).ToArray();
//                    updateCmd = ctor.Invoke(args);
//                }
//            }

//            var id = Guid.NewGuid();
//            var result = await controller.Update(id, (dynamic)updateCmd!, CancellationToken.None);
//            Assert.IsType<NotFoundResult>(result);
//        }

//        [Fact]
//        public async Task Delete_ReturnsNoContent_WhenDeleted()
//        {
//            var responses = new Dictionary<string, object?> { ["DeleteEventCommand"] = true };
//            var controller = new EventsController(new TestSender(responses));
//            var id = Guid.NewGuid();
//            var result = await controller.Delete(id, CancellationToken.None);
//            Assert.IsType<NoContentResult>(result);
//        }

//        [Fact]
//        public async Task Delete_ReturnsNotFound_WhenMissing()
//        {
//            var responses = new Dictionary<string, object?> { ["DeleteEventCommand"] = false };
//            var controller = new EventsController(new TestSender(responses));
//            var id = Guid.NewGuid();
//            var result = await controller.Delete(id, CancellationToken.None);
//            Assert.IsType<NotFoundResult>(result);
//        }
//    }
//}
