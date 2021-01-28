using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Interacts with the Books table in BookStore's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;


        public BooksController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Get all books from Database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Attempted call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location} : Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Get an Book by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Book's Record</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($" {location} : Attempted Get Book by ID : {id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($" {location} : Book with ID : {id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo($"Successfully Got Book by ID : {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles ="Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDtO)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Author Submission Attempted.");

                if (bookDtO == null)
                {
                    _logger.LogWarn($"{location} : Empty request was submitted..!");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location} : Author Data was incomplete..!");
                    return BadRequest(ModelState);
                }


                var book = _mapper.Map<Book>(bookDtO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                {
                    return InternalError($"{location} : Author Creation Failed..!");
                }

                _logger.LogInfo("{location} : Author created successfully.");
                return Created("Create", new { book });

            }
            catch (Exception e)
            {

                return InternalError($"{location} : {e.Message} - {e.InnerException}");

            }
        }


        /// <summary>
        /// Updates the Book record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Book Update Attempted with id : {id}");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogWarn($"{location} : Book updation failed.");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location} : Book data was incomplete.");
                    return BadRequest(ModelState);
                }
                var isExist = await _bookRepository.IsExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location} : Book with id : {id} was not found.");
                    return NotFound();
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);

                if (!isSuccess)
                {
                    return InternalError("{location} : Update operation failed.");
                }
                _logger.LogInfo($"{location} : Book data was updated successfully.");
                return NoContent();
            }
            catch (Exception e)
            {

                return InternalError($"{location} : {e.Message} - {e.InnerException}");

            }

        }


        /// <summary>
        /// Deletes the book record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Book Delete Attempted with id : {id}");
                if (id < 1)
                {
                    _logger.LogWarn($"{location} : Book Delete Failed");
                    return BadRequest();
                }

                var isExist = await _bookRepository.IsExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location} : Book with id : {id} was not found.");
                    return NotFound();
                }

                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"{location} : Book Delete failed.");
                }
                _logger.LogInfo($"{location} : Book data was deleted successfully.");
                return NoContent();


            }
            catch (Exception e)
            {

                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }









        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Somethiing went wrong, Please contact to Admin....!!");
        }


        private string GetControllerNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller}-{action}";

        }

    }
}
