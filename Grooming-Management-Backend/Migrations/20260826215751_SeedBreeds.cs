using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class SeedBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
    table: "Breeds",
    column: "Name",
    values: new object[]
    {
        "Yorkshire Terrier",
        "Shih Tzu",
        "Maltańczyk",
        "Pudel Miniaturowy",
        "Sznaucer Miniaturowy",
        "West Highland White Terrier",
        "Pudel Toy",
        "Bichon Frise",
        "Cocker Spaniel Angielski",
        "Szpic Miniaturowy (Pomeranian)",
        "Hawańczyk",
        "Cavalier King Charles Spaniel",
        "Pudel Średni",
        "Lhasa Apso",
        "Golden Retriever",
        "Labrador Retriever",
        "Sznaucer Średni",
        "Bolończyk",
        "Cocker Spaniel Amerykański",
        "Pekińczyk",
        "Chiński Grzywacz",
        "Jack Russell Terrier",
        "Terier Tybetański",
        "Coton de Tuléar",
        "Spaniel Kontynentalny Miniaturowy",
        "Gryfonik Brukselski",
        "Gryfonik Belgijski",
        "Brabantczyk",
        "Pudel Duży",
        "Berneński Pies Pasterski",
        "Samojed",
        "Owczarek Australijski (typ amerykański)",
        "Border Collie",
        "Owczarek Szetlandzki",
        "Owczarek Niemiecki",
        "Nowofundland",
        "Chow Chow",
        "Shiba",
        "Mops",
        "Buldog Francuski",
        "Chihuahua",
        "Boston Terrier",
        "Sznaucer Olbrzym",
        "Airedale Terrier",
        "Foksterier Szorstkowłosy",
        "Terier Szkocki",
        "Terier Walijski",
        "Terier Irlandzki",
        "Kerry Blue Terrier",
        "Irish Soft Coated Wheaten Terrier",
        "Cairn Terrier",
        "Border Terrier",
        "Norfolk Terrier",
        "Norwich Terrier",
        "Australian Silky Terrier",
        "Yorkshire Terrier Biewer",
        "Lagotto Romagnolo",
        "Barbet",
        "Portugalski Pies Dowodny",
        "Hiszpański Pies Dowodny",
        "Fryzyjski Pies Dowodny",
        "Amerykański Spaniel Dowodny",
        "Curly Coated Retriever",
        "Flat Coated Retriever",
        "Nova Scotia Duck Tolling Retriever",
        "Chesapeake Bay Retriever",
        "Springer Spaniel Angielski",
        "Springer Spaniel Walijski",
        "Field Spaniel",
        "Clumber Spaniel",
        "Kooikerhondje",
        "Płochacz Niemiecki",
        "Bearded Collie",
        "Polski Owczarek Nizinny",
        "Polski Owczarek Podhalański",
        "Owczarek Belgijski",
        "Biały Owczarek Szwajcarski",
        "Welsh Corgi Pembroke",
        "Welsh Corgi Cardigan",
        "Pumi",
        "Puli",
        "Komondor",
        "Owczarek Staroangielski Bobtail",
        "Bergamasco",
        "Schapendoes",
        "Leonberger",
        "Hovawart",
        "Bernardyn",
        "Akita",
        "Eurasier",
        "Szpic Japoński",
        "Szpic Włoski",
        "Lwi Piesek",
        "Rosyjski Toy",
        "Chin Japoński",
        "Chart Afgański",
        "Dalmatyńczyk",
        "Rhodesian Ridgeback",
        "Beagle",
        "Sussex Spaniel",
        "Mieszaniec",
        "Rasa nieznana",
    });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
