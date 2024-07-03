using System.Security.Claims;
using EventManagementApi.DTO;
using EventManagementApi.Entities;
using EventManagementApi.Entity;
using EventManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private readonly ServiceBusQueueService _serviceBusQueueService;

        public EventsController(ApplicationDbContext context, BlobStorageService blobStorageService, ServiceBusQueueService serviceBusQueueService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _serviceBusQueueService = serviceBusQueueService;
        }

        // POST: api/v1/Events
        [HttpPost]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newEvent = new Event
            {
                Name = eventDto.Name,
                Description = eventDto.Description,
                Location = eventDto.Location,
                Date = eventDto.Date,
                OrganizerId = eventDto.OrganizerId,
                TotalSpots = eventDto.TotalSpots,
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }


        // GET: api/v1/Events
        [HttpGet]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events);
        }

        // GET: api/v1/Events/id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return Ok(eventItem);
        }

        // PUT: api/v1/Events/id
        [HttpPut("{id}")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UpdateEvent(string id, [FromBody] EventUpdateDto eventDto)
        {
            if (!Guid.TryParse(id, out var eventId))
            {
                return BadRequest("Invalid event ID");
            }

            var existingEvent = await _context.Events.FindAsync(eventId);
            if (existingEvent == null)
            {
                return NotFound();
            }

            existingEvent.Name = eventDto.Name ?? existingEvent.Name;
            existingEvent.Description = eventDto.Description ?? existingEvent.Description;
            existingEvent.Location = eventDto.Location ?? existingEvent.Location;
            existingEvent.Date = eventDto.Date ?? existingEvent.Date;
            existingEvent.OrganizerId = eventDto.OrganizerId ?? existingEvent.OrganizerId;
            existingEvent.TotalSpots = eventDto.TotalSpots ?? existingEvent.TotalSpots;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Events.Any(e => e.Id == eventId))
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        // DELETE: api/v1/Events/id
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // POST: api/v1/Events/id/register
        [HttpPost("{id}/register")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> RegisterForEvent(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var eventExists = await _context.Events.AnyAsync(e => e.Id.ToString() == id);
            if (!eventExists)
            {
                return NotFound();
            }

            var registration = new EventRegistrationDto
            {
                EventId = id.ToString(),
                UserId = userId,
                Action = "Register"
            };

            await _context.SaveChangesAsync();

            var message = new ServiceBusMessage(new BinaryData(JsonConvert.SerializeObject(registration)))
            {
                SessionId = id.ToString()
            };


            await _serviceBusQueueService.SendMessageAsync(message);
            return Accepted(new { message = "Registration request submitted" });
        }


        // Delete api/v1/Events/id/unregister
        [HttpDelete("{id}/unregister")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> UnregisterFromEvent(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             Console.WriteLine(id);
            // Check if the registration exists
            var registrationExists = await _context.EventRegistrations.AnyAsync(r => r.EventId == id && r.UserId == userId);
            if (!registrationExists)
            {
                return NotFound(new { message = "Registration not found" });
            }

            var registration = new EventRegistrationDto
            {
                EventId = id.ToString(),
                UserId = userId,
                Action = "Unregister"
            };

            var message = new ServiceBusMessage(new BinaryData(System.Text.Json.JsonSerializer.Serialize(registration)))
            {
                SessionId = id.ToString()
            };

            await _serviceBusQueueService.SendMessageAsync(message);
            return Accepted(new { message = "Unregistration request submitted" });
        }


     // api/v1/Events/id/upload-images
        [HttpPost("{id}/upload-images")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UploadImages(Guid id, List<IFormFile> imageFiles)
        {
            if (imageFiles == null || imageFiles.Count == 0)
            {
                return BadRequest("No files provided.");
            }

            var eventEntity = await _context.Events.Include(e => e.Images).FirstOrDefaultAsync(e => e.Id == id);
            if (eventEntity == null)
            {
                return NotFound("Event not found.");
            }

            foreach (var file in imageFiles)
            {
                var imageUrl = await _blobStorageService.UploadFileAsync(file, "eventimages");
                eventEntity.Images.Add(new EventImage { Url = imageUrl });
            }

            await _context.SaveChangesAsync();

            return Ok(eventEntity.Images.Select(img => img.Url));
        }

       // api/v1/Events/id/upload-documents
        [HttpPost("{id}/upload-documents")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UploadDocuments(Guid id, List<IFormFile> documentFiles)
        {
            if (documentFiles == null || documentFiles.Count == 0)
            {
                return BadRequest("No documents provided.");
            }

            var eventEntity = await _context.Events.Include(e => e.Documents).FirstOrDefaultAsync(e => e.Id == id);
            if (eventEntity == null)
            {
                return NotFound("Event not found.");
            }

            foreach (var file in documentFiles)
            {
                if (!IsSupportedDocument(file.FileName))
                {
                    return BadRequest("Unsupported file type.");
                }

                var documentUrl = await _blobStorageService.UploadFileAsync(file, "eventdocuments");
                eventEntity.Documents.Add(new EventDocument { Url = documentUrl });
            }

            await _context.SaveChangesAsync();

            return Ok(eventEntity.Documents.Select(doc => doc.Url));
        }
      
        private bool IsSupportedDocument(string fileName)
        {
            var supportedTypes = new[] { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "csv" };
            var fileExtension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            return supportedTypes.Contains(fileExtension);
        }
    }
}