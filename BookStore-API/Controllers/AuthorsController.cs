using System;
using System.Collections.Generic;
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
    /// Endpoint used to interact to the Authors in BookStore's Database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;


        public AuthorsController(IAuthorRepository authorRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all authors from Database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
       
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Attempted call");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Successfully Got All Authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get an Author by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Author's Record</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Attempted Get Author by ID : {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"{location} :Author with ID : {id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($" {location} : Successfully Got Author by ID : {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles ="Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Author Submission Attempted.");

                if (authorDTO == null)
                {
                    _logger.LogWarn($"{location} : Empty request was submitted..!");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location} : Author Data was incomplete..!");
                    return BadRequest(ModelState);
                }


                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
                if (!isSuccess)
                {
                    //_logger.LogWarn($"Author Creation Failed..!");
                    return InternalError($"{location} : Author Creation Failed..!");
                }

                _logger.LogInfo("{location} : Author created successfully.");
                return Created("Create", new { author });

            }
            catch (Exception e)
            {

                return InternalError($"{location} : {e.Message} - {e.InnerException}");

            }
        }



        /// <summary>
        /// Updates the Author record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles ="Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetControllerNames();
            try
            {
                _logger.LogInfo($"{location} : Author Update Attempted with id : {id}");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"{location} : Author updation failed.");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location} : Author data was incomplete.");
                    return BadRequest(ModelState);
                }
                var isExist = await _authorRepository.IsExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location} : Author with id : {id} was not found.");
                    return NotFound();
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);

                if (!isSuccess)
                {
                    return InternalError("{location} : Update operation failed.");
                }
                _logger.LogInfo($"{location} : Author data was updated successfully.");
                return NoContent();
            }
            catch (Exception e)
            {

                return InternalError($"{location} : {e.Message} - {e.InnerException}");

            }

        }



        /// <summary>
        /// Deletes the Author record
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
                _logger.LogInfo($"{location} : Author Delete Attempted with id : {id}");
                if (id < 1)
                {
                    _logger.LogWarn($"{location} : Author Delete Failed");
                    return BadRequest();
                }

                var isExist = await _authorRepository.IsExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location} : Author with id : {id} was not found.");
                    return NotFound();
                }

                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"{location} : Author Delete failed.");
                }
                _logger.LogInfo($"{location} : Author data was deleted successfully.");
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
