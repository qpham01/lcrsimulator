using Moq;

namespace LCRLogic.Tests
{
    public class LCRGameTests
    {
        [Test]
        public void TestGameWithAllCenterRolls()
        {
            var rolls = new string[] { LCRDice.C, LCRDice.C, LCRDice.C };
            var mockDice = new Mock<ILCRDice>();
            mockDice.Setup(x => x.Roll(It.IsAny<int>())).Returns(rolls);
            var sut = new LCRGame(mockDice.Object);
            sut.PlayGame(3);
            Assert.That(sut.Winner?.Index, Is.EqualTo(2));
            Assert.That(sut.CenterPot, Is.EqualTo(6));
            Assert.That(sut.TurnCount, Is.EqualTo(3));
        }

        [Test]
        public void TestGameWhereChipsGoesToPlayer0()
        {
            var rolls = new string[] { LCRDice.Dot, LCRDice.L, LCRDice.Dot, LCRDice.C, LCRDice.L, LCRDice.L, LCRDice.R, LCRDice.R, LCRDice.R, LCRDice.Dot, LCRDice.Dot, LCRDice.Dot, LCRDice.R };
            var fixedDice = new LCRFixedDice(rolls);            
            var sut = new LCRGame(fixedDice);
            sut.PlayGame(3);
            Assert.That(sut.CenterPot, Is.EqualTo(1));
            Assert.That(sut.Winner?.Index, Is.EqualTo(0));
            Assert.That(sut.Winner?.ChipCount, Is.EqualTo(8));
            Assert.That(sut.TurnCount, Is.EqualTo(7));
        }

        [Test]
        public void TestGameWhereChipsGoesToPlayer1()
        {
            var rolls = new string[] { LCRDice.C, LCRDice.R, LCRDice.R, LCRDice.C, LCRDice.Dot, LCRDice.Dot, LCRDice.L, LCRDice.L, LCRDice.C };
            var fixedDice = new LCRFixedDice(rolls);
            var sut = new LCRGame(fixedDice);
            sut.PlayGame(3);
            Assert.That(sut.CenterPot, Is.EqualTo(3));
            Assert.That(sut.Winner?.Index, Is.EqualTo(1));
            Assert.That(sut.Winner?.ChipCount, Is.EqualTo(6));
            Assert.That(sut.TurnCount, Is.EqualTo(5));
        }

    }
}