﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Models;
using RestaurantAPI.Data;
using System.Threading.Tasks;
using System.Globalization;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    public class Dish_IngredientController : Controller
    {

        private readonly Dish_IngredientRepository _repository;
        private readonly DishRepository _dishRepository;
        private TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        private DishController _dishController;

        public Dish_IngredientController(Dish_IngredientRepository repository, DishRepository dishRepository, DishController dishController)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
            _dishController = dishController ?? throw new ArgumentNullException(nameof(dishController));
        }

        // GET: api/dish_ingredient
        [HttpGet]
        public async Task<List<Dish_Ingredient>> Get()
        {
            // Getting all records from the Customer table
            return await _repository.GetAll();
        }

        // GET api/dish_ingredient/5/potato
        [HttpGet("{dish_id}/{ing_name}")]
        public async Task<ActionResult<Dish_Ingredient>> Get(int dish_id, string ing_name)
        {
            ing_name = textInfo.ToTitleCase(ing_name.ToLower());

            try
            {
                // Searching for record in the database
                var response = await _repository.GetById(dish_id, ing_name);
                return response;
            }
            catch (Npgsql.PostgresException ex)
            {
                // Postgres threw an exceptions
                return BadRequest(ex.Message.ToString());
            }
            catch
            {
                // Unknown error
                return NotFound("Record you are searching for does not exist");
            }
        }

        // POST api/dish_ingredient
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Dish_Ingredient dish_ingredient)
        {
            dish_ingredient.Ing_Name = textInfo.ToTitleCase(dish_ingredient.Ing_Name.ToLower());
            
            try
            {
                // Inserting record in the Dish_Ingredient table
                await _repository.Insert(dish_ingredient);
                return Ok("Record inserted successfully\n");
            }
            catch (Npgsql.PostgresException ex)
            {
                // Postgres threw an exception
                return BadRequest(ex.Message.ToString());

            }
            catch
            {
                // Unknown error
                return BadRequest("Error: Record was not inserted\n");
            }
        }

        // PUT api/dish_ingredient
        [HttpPut]
        public ActionResult Put()
        {
            // We cannot add any modify entries in the Dish_Ingredient table. It has to be done directly through deletes and posts
            return BadRequest("ERROR: You cannot modify entries int the Ingredient_Table. Try using POST and DELETE instead.\n");
        }

        // DELETE api/dish_ingredient/5/potato
        [HttpDelete("{dish_id}/{ing_name}")]
        public async Task<ActionResult> Delete(int dish_id, string ing_name)
        {
            ing_name = textInfo.ToTitleCase(ing_name.ToLower());

            try
            {
                // Searching for record inn the Dish_Ingredient table
                var response = await _repository.GetById(dish_id, ing_name);

                // If last ingredient from dish is removed -> remove dish as well
                if (await _repository.numIngredientsInDish(dish_id) == 1)
                {
                    await _repository.DeleteById(dish_id, ing_name);
                    return await _dishController.Delete(dish_id);
                }
                else // Remove record in the Dish_Ingredient table
                {
                    await _repository.DeleteById(dish_id, ing_name);
                    string format = "Record with key={0},{1} deleted succesfully\n";
                    return Ok(string.Format(format, dish_id, ing_name));
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                // Postgres threw an exception
                return BadRequest(ex.Message.ToString());
            }
            catch
            {
                // Unknown error
                return BadRequest("Error: Record could not be deleted\n");
            }
        }

        // api/order_dish/getNumDishes/2
        [Route("numIngredientsInDish/{dish_id}")]
        [HttpGet]
        public async Task<ActionResult> numIngredientsInDish(int dish_id)
        {
            try
            {
                // There is no error and we are able to retrieve the number of ingredients for the specified dish
                string format = "The number of ingredients in dish = {0} is {1}\n";
                return Ok(string.Format(format, dish_id, await _repository.numIngredientsInDish(dish_id)));
        }
            catch (Npgsql.PostgresException ex)
            {
                // Postgres threw an exception
                return BadRequest(ex.Message.ToString());
            }
            catch
            {
                // Some unknown exception
                return BadRequest("ERROR: Number of ingredients for that record could not be retrieved");
            }
        }
    }
}
