using ChangeAppsettingsInRealTime.Services;
using FluentAssertions;
using ndd.SharedKernel.Result;
using NUnit.Framework;
using System;
using System.IO;

namespace ChangeAppsettingsInRealTimeTests.Services
{
    [TestFixture]
    public class AppsettingsServiceTests
    {
        private IAppsettingsService _appsettingsService;
        private string _filePath;

        [SetUp]
        public void Initialize()
        {
            _appsettingsService = new AppsettingsService();

            var workingDirectory = Environment.CurrentDirectory;
            _filePath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        }

        [Test]
        public void Escrever_Com_TresCamadasDeProfundidadeENumeroInteiro_Deve_ObterSucesso()
        {
            //Arrange    
            var key = "Config:Teste:Teste1:Repeat";

            Random randomNumber = new Random();
            var value = randomNumber.Next(1, 100);

            //Action
            var result = _appsettingsService.Write(key, value, _filePath);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Success.Should().BeOfType<Unit>();
        }

        [Test]
        public void Escrever_Com_TresCamadasDeProfundidadeEString_Deve_ObterSucesso()
        {
            //Arrange    
            var key = "Config:Teste:Teste1:Person";

            Random randomWords = new Random();
            string[] people = new string[] { "dev", "world", "person", "dinosaur" };
            var value = people[randomWords.Next(0, people.Length)];

            //Action
            var result = _appsettingsService.Write(key, value, _filePath);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Success.Should().BeOfType<Unit>();
        }

        [Test]
        public void Escrever_Com_TresCamadasDeProfundidadeEBoolean_Deve_ObterSucesso()
        {
            //Arrange    
            var key = "Config:Teste:Teste1:Save";

            Random randomBoolean = new Random();
            var value = randomBoolean.Next(1, 10) <= 5 ? true : false;

            //Action
            var result = _appsettingsService.Write(key, value, _filePath);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Success.Should().BeOfType<Unit>();
        }
    }
}
