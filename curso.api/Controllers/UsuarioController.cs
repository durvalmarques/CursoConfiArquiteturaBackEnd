﻿using curso.api.Business.Entities;
using curso.api.Business.Repositories;
using curso.api.Configurations;
using curso.api.Filters;
using curso.api.Models;
using curso.api.Models.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace curso.api.Controllers
{
    [Route("api/v1/usuario")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;
        public UsuarioController(
            IUsuarioRepository usuarioRepository,
            IConfiguration Configuration,
            IAuthenticationService authenticationService)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = Configuration;
            _authenticationService = authenticationService;

        }

      
        /// <summary>
        /// Este serviço permite autenticar um usuário cadastrado e ativo.
        /// </summary>
        /// <param name="loginViewModelInput">View model do login</param>
        /// <returns>Retorna status ok, dados do usuario e o token em caso de sucesso</returns>
        [SwaggerResponse(statusCode: 200, description: "Sucesso ao autenticar", Type = typeof(LoginViewModelOutput))]
        [SwaggerResponse(statusCode: 400, description: "Campos obrigatórios", Type = typeof(ValidaCampoViewModelOutput))]
        [SwaggerResponse(statusCode: 500, description: "Erro interno", Type = typeof(ErroGenericoViewModel))]

        [HttpPost]
        [Route("logar")]
        [ValidacaoModelStateCustomizado]
        public IActionResult Logar(LoginViewModelInput loginViewModelInput)
        {
            
            var usuario = _usuarioRepository.ObterUsuario(loginViewModelInput.Login);

            if (usuario == null)
            {
                return BadRequest("Houve um erro ao tentar acessar.");
            }

            //if (usuario.Senha != loginViewModel.Senha.GerarSenhaCriptografada())
            //{
            //    return BadRequest("Houve um erro ao tentar acessar.");
            //}

            var usuarioViewModelOutput = new UsuarioViewModelOutput()
            {
                Codigo = usuario.Codigo,
                Login = loginViewModelInput.Login,
                Email = usuario.Email
            };

            var token = _authenticationService.GerarToken(usuarioViewModelOutput);

            return Ok(new LoginViewModelOutput
            {
                Token = token,
                Usuario = usuarioViewModelOutput
            });


            

        }

        /// <summary>
        /// Este serviço permite cadastrar um usuário cadastrado não existente.
        /// </summary>
        /// <param name="loginViewModelInput">View model do registro de login</param>
        [SwaggerResponse(statusCode: 201, description: "Sucesso ao cadastrar", Type = typeof(RegistroViewModelInput))]
        [SwaggerResponse(statusCode: 400, description: "Campos obrigatórios", Type = typeof(ValidaCampoViewModelOutput))]
        [SwaggerResponse(statusCode: 500, description: "Erro interno", Type = typeof(ErroGenericoViewModel))]

        [HttpPost]
        [Route("registrar")]
        [ValidacaoModelStateCustomizado]
        public IActionResult Registrar(RegistroViewModelInput loginViewModelInput)
        {

            //var optionsBuilder = new DbContextOptionsBuilder<CursoDbContext>();
            //optionsBuilder.UseSqlServer(connectionString: @"Server=(localdb)\mssqllocaldb;Database=CursoApi;Integrated Security=True");
            //CursoDbContext contexto = new CursoDbContext(optionsBuilder.Options);

            //var migracoesPendentes = contexto.Database.GetPendingMigrations();

            //if (migracoesPendentes.Count() > 0)
            //{
            //    contexto.Database.Migrate();
            //}

            var usuario = new Usuario();
            usuario.Login = loginViewModelInput.Login;
            usuario.Senha = loginViewModelInput.Senha;
            usuario.Email = loginViewModelInput.Email;
            _usuarioRepository.Adicionar(usuario);
            _usuarioRepository.Commit();

            return Created("", loginViewModelInput);

        }
    }
}


