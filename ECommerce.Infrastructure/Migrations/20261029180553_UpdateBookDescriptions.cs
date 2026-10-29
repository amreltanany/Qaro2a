using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "رواية فانتازيا بأبعاد فلسفية ونفسية. بين «عبّار» و«فارق» تجد نفسك في رحلة تواجه فيها كل ما تهرب منه، بلا سبيل للفرار من الحقيقة. رحلة مليئة بالخبايا والأسرار بين الحقيقة والزيف، والتعلّق والهروب، هدفها الوصول إلى الحقيقة وحدها.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "تجربة فانتازيا ليست بعيدة عن واقعنا؛ فالحياة مليئة باختبارات سهلة وصعبة، وما يبدو سهلًا قد يحمل جوهر الصعوبة. بين الاختيارات ومعرفة الحقيقة التي تتغيّر مع زيادة الوعي تتشكّل رحلتنا في «أرخبيل الملوك»: رحلة عالية المخاطر بين البقاء والتحرر، تجمع الحركة المكثفة بالأسئلة الفلسفية العميقة.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "بين أعباء الحياة وسباتها المستمر والتوغل في أحلام الآخرين، نفقد جزءًا من أرواحنا ونغدو أحياءً بأجساد هامدة. لكن ماذا لو قررت كسر هذه الدائرة وبدأت رحلتك وفق قواعدك وأحلامك؟ رحلة تكون فيها البطل، تبحث خلالها عن معنى لحياتك وعن ذاتك الحقيقية.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "تنقلب حياة طبيب نفسي ناجح بعد وفاة والدته، الرسامة الشهيرة المنعزلة، حين يكتشف بالصدفة تسجيلات قديمة ودفاتر تخفي أسرارًا لم يعرفها عنها من قبل. رواية عن الفقد والذاكرة، وعمّا تخبئه اللوحات خلف ألوانها.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Author", "Description" },
                values: new object[] { "ممدوح عماد، نوران بسيوني، رانا رؤوف، آية المنشاوي", "ليست مجموعة قصص وحسب، بل نسيج متشابك بين الدراما والفانتازيا: رحلة بخمس تجارب مختلفة مع الشعور بالذنب، حين يتحول من سؤال بريء ينم عن رغبة صاحبه في أن يصبح شخصًا أفضل إلى حلقة مفرغة لا تنتهي من شعور يأكله ببطء. ليس بالضرورة أن تدخل حلقة الذنب بقرار واعٍ." });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Author", "Description" },
                values: new object[] { "رانا رؤوف", "كل مرة تتعرض فيها للرفض وتنغلق على نفسك وتسألها: لماذا اخترت أن أكون في هذا المكان؟ تجد إجابة واحدة في انتظارك كل مرة. رواية عن الرفض والإصرار، وعن البحث عن مكانك الحقيقي." });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: "رواية رومانسية ديستوبية من عالم سلسلة Shatter Me للكاتبة طاهرة ماڤي، ترجمة ضحى صلاح. حكاية عن حب يولد في عالم قاسٍ، وقوى خارقة تخيف أصحابها قبل الآخرين، وعن شجاعة أن تتحرر من الخوف.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                column: "Description",
                value: "كتاب ترفيهي للكبار يجمع الألعاب والألغاز والتحديات الذهنية، لساعات من التسلية وشحذ الذكاء والتركيز. يناسب القراءة الفردية أو قضاء وقت ممتع مع الأصدقاء والعائلة.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                column: "Description",
                value: "رواية فلسفية للكاتب الليبي إبراهيم الكوني عن حب يتحدّى قوانين القبيلة الصارمة، فلا يبقى أمام البطل إلا الطاعة والتخلّي أو السقوط في «الفم»؛ الهاوية التي تبتلع من يتمرّد على قدر الصحراء. رحلة تتداخل فيها التضحية بصمت الرمال.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                column: "Description",
                value: "ديسمبر 2003: يُعثر على زوج أم المخرج السينمائي غاي هوارد ميتًا في حديقة منزل العائلة في لندن، وتقول والدته ونصفا شقيقتيه إنهن لم يرين شيئًا. بعد سنوات يعيد مسلسل وثائقي فتح القضية فتتكشف الأسرار أمام الجميع، وتبدو الحقيقة أقرب إلى البيت مما يُتوقع. رواية جريمة وتشويق بأسلوب الرسائل والمقابلات والوثائق.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                column: "Description",
                value: "ريم امرأة تعيش وحدها وتواجه الألم والفقد، إلى أن تضعها مواجهة غير متوقعة أمام أسطورة قديمة تتداولها الحكايات، فتبدأ في تتبّع خيوط الحقيقة. رواية تمزج الغموض بالخيال، بنهاية تحمل مفاجأة.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                column: "Description",
                value: "ساعة الذئب هي الساعة التي تسبق الفجر، حين يبلغ الظلام أشدّه ويبدو كل شيء ممكنًا. رواية رعب وتشويق للكاتب خالد أمين، من إصدارات دار كيان للنشر.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "بعد فارق — نوران بسيوني. من إصدارات قروءه.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "أرخبيل الملوك — ممدوح عماد. من إصدارات قروءه.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "طيف نجاة — آية المنشاوي. من إصدارات قروءه.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "لوحة أخيرة — علياء المنشاوي. من إصدارات قروءه.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Author", "Description" },
                values: new object[] { "ممدوح عماد، نوران بسيوني، رنا رؤوف، آية المنشاوي", "حلقة الذنب — ممدوح عماد، نوران بسيوني، رنا رؤوف، آية المنشاوي. من إصدارات قروءه." });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Author", "Description" },
                values: new object[] { "رنا رؤوف", "طلبك مرفوض — رنا رؤوف. من إصدارات قروءه." });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: "رواية رومانسية للكاتبة طاهرة ماڤي، ترجمة ضحى صلاح، قصة مشاعر وتحوّلات تحمل الكثير من التشويق.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                column: "Description",
                value: "ألعاب وألغاز وتسالي للكبار");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                column: "Description",
                value: "عمل أدبي فلسفي معاصر للكاتب إبراهيم الكوني، من إصدارات دار كيان للنشر.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                column: "Description",
                value: "رواية جريمة وإثارة وتشويق مترجمة للكاتبة كارا هانتر، تحقيق بوليسي تتكشّف فيه الأسرار خلف الأبواب المغلقة لعائلة واحدة.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                column: "Description",
                value: "أقسى من الموت — محمد عصمت. من إصدارات دار كيان للنشر.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                column: "Description",
                value: "ساعة الذئب — خالد أمين. من إصدارات دار كيان للنشر.");
        }
    }
}
