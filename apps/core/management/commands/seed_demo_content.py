import io
from datetime import timedelta
from decimal import Decimal

from django.contrib.auth import get_user_model
from django.core.files.base import ContentFile
from django.core.management.base import BaseCommand
from django.utils import timezone
from PIL import Image

from apps.blog.models import Category, Post
from apps.core.models import SiteSetting
from apps.leads.models import LeadMagnet
from apps.navigation.models import NavItem
from apps.offerings.models import Offering, Session
from apps.pages.models import FlatPage
from apps.testimonials.models import Testimonial

User = get_user_model()

# (top color, bottom color) — a few pleasant, on-brand-ish gradients cycled
# across cards so the grids don't look like a wall of identical placeholders.
GRADIENTS = [
    ((15, 118, 110), (217, 119, 6)),
    ((30, 64, 175), (147, 51, 234)),
    ((190, 24, 93), (249, 115, 22)),
    ((5, 150, 105), (13, 148, 136)),
    ((124, 58, 237), (219, 39, 119)),
    ((202, 138, 4), (220, 38, 38)),
]


def make_gradient_png(index, size=(900, 560)):
    top, bottom = GRADIENTS[index % len(GRADIENTS)]
    width, height = size
    base = Image.new("RGB", size, top)
    overlay = Image.new("RGB", size, bottom)
    mask = Image.new("L", size)
    mask.putdata([int(255 * (y / height)) for y in range(height) for _ in range(width)])
    base.paste(overlay, (0, 0), mask)
    buf = io.BytesIO()
    base.save(buf, format="PNG")
    return buf.getvalue()


def make_fake_pdf(title):
    body = f"%PDF-1.4\n% mock demo file for \"{title}\"\n%%EOF".encode()
    return body


class Command(BaseCommand):
    help = "Seeds realistic bilingual (fa/ckb) demo content across every public page, and publishes the phased-rollout sections so they're all reachable."

    def handle(self, *args, **options):
        # Prefer a superuser with a real email over a bare bootstrap account
        # (e.g. a generic "admin" from initial setup with no email/name set)
        # — otherwise every seeded post ends up authored by a placeholder.
        superusers = User.objects.filter(is_superuser=True).order_by("pk")
        author = superusers.exclude(email="").first() or superusers.first()
        if not author:
            self.stdout.write(self.style.ERROR("No user found — create one first (createsuperuser)."))
            return

        self._seed_site_settings()
        categories = self._seed_categories()
        self._seed_posts(author, categories)
        offerings = self._seed_offerings()
        self._seed_testimonials(offerings)
        self._seed_leads()
        self._seed_pages()
        self._publish_sections()

        self.stdout.write(self.style.SUCCESS("Demo content seeded. All phased-rollout sections are now published."))

    def _seed_site_settings(self):
        # SiteSetting.site_name / meta_description are plain (non-translated)
        # fields — unlike Post/Offering/FlatPage, there's no _fa/_ckb split here.
        settings_obj = SiteSetting.load()
        settings_obj.site_name = "آرش بختیاری"
        settings_obj.meta_description = (
            "یادداشت‌ها، دوره‌های آموزشی و تجربه‌های واقعی درباره‌ی توسعه وب، طراحی و کسب‌وکار — به فارسی و کردی سورانی."
        )
        settings_obj.contact_email = "hello@arashbakhtiari.example"
        settings_obj.contact_phone = "+98 912 000 0000"
        settings_obj.social_links = {
            "instagram": "https://instagram.com/arashbakhtiari",
            "telegram": "https://t.me/arashbakhtiari",
            "linkedin": "https://linkedin.com/in/arashbakhtiari",
        }
        settings_obj.save()

    def _seed_categories(self):
        specs = [
            ("web-dev", "توسعه وب", "گەشەپێدانی وێب"),
            ("design", "طراحی محصول", "دیزاینی بەرهەم"),
            ("business", "کسب‌وکار", "بازرگانی"),
        ]
        categories = {}
        for slug, name_fa, name_ckb in specs:
            category, _created = Category.objects.get_or_create(
                slug=slug, defaults={"name_fa": name_fa, "name_ckb": name_ckb}
            )
            categories[slug] = category
        return categories

    def _seed_posts(self, author, categories):
        posts = [
            dict(
                slug="django-vs-fastapi-2026",
                category=categories["web-dev"],
                tags=["Django", "Python", "بک‌اند"],
                title_fa="جنگو یا FastAPI؟ تجربه‌ی واقعی بعد از سه پروژه",
                title_ckb="جانگۆ یان FastAPI؟ ئەزموونی ڕاستەقینە دوای سێ پڕۆژە",
                excerpt_fa="بعد از سه پروژه‌ی واقعی، این چیزیه که واقعاً بین این دو فریم‌ورک فرق می‌کنه — نه چیزی که در مقایسه‌های سطحی می‌خونید.",
                excerpt_ckb="دوای سێ پڕۆژەی ڕاستەقینە، ئەمە ئەو شتەیە کە ڕاستی جیاوازی لەنێوان ئەم دوو چوارچێوەیە — نەک ئەوەی لە بەراوردە سەرەکییەکاندا دەیبینیت.",
                body_fa="""
<p>وقتی اولین بار FastAPI رو امتحان کردم، فکر می‌کردم دیگه هیچ‌وقت سراغ جنگو نمی‌رم. سرعت توسعه، type hints، مستندات خودکار — همه‌چیز عالی به نظر می‌رسید. اما بعد از سه پروژه‌ی واقعی، تصویر پیچیده‌تر شد.</p>
<h2>جایی که FastAPI برنده می‌شه</h2>
<ul>
<li>API‌های سبک و مستقل که نیازی به پنل مدیریت ندارن</li>
<li>پروژه‌هایی که async از ابتدا بخش اصلی معماریشونه</li>
<li>تیم‌های کوچیک که سرعت اولیه مهم‌تر از ساختار بلندمدته</li>
</ul>
<h2>جایی که جنگو هنوز میدون‌داره</h2>
<p>هر وقت پروژه شامل مدل‌های داده‌ی پیچیده، پنل مدیریت واقعی، یا چند اپلیکیشن به‌هم‌متصل باشه، جنگو زمان زیادی رو صرفه‌جویی می‌کنه. ORM جنگو، سیستم migration، و اکوسیستم بسته‌هاش (django-otp، modeltranslation، تگ‌گذاری) چیزهاییه که در FastAPI باید از صفر بسازی یا از چند کتابخونه‌ی جدا وصلشون کنی.</p>
<blockquote>نتیجه‌ای که به‌ش رسیدم: FastAPI رو برای میکروسرویس‌ها انتخاب می‌کنم، جنگو رو برای هر چیزی که «محصول» باشه، نه فقط یک API.</blockquote>
""".strip(),
                body_ckb="""
<p>یەکەم جار کە FastAPI تاقیم کردەوە، وا بیر دەکردمەوە کە چیتر ناچمە سەر جانگۆ. خێرایی گەشەپێدان، type hints، بەڵگەنامەی ئۆتۆماتیکی — هەموو شتێک نایاب دەرکەوت. بەڵام دوای سێ پڕۆژەی ڕاستەقینە، وێنەکە ئاڵۆزتر بوو.</p>
<h2>کوێ FastAPI سەرکەوتووە</h2>
<ul>
<li>API‌ی سووک و سەربەخۆ کە پێویستی بە پانێلی بەڕێوەبردن نییە</li>
<li>پڕۆژەکان کە async لە سەرەتاوە بەشی سەرەکی پێکهاتەیانە</li>
<li>تیمی بچووک کە خێرایی سەرەتایی گرنگترە لە پێکهاتەی درێژخایەن</li>
</ul>
<h2>کوێ جانگۆ هێشتا زاڵە</h2>
<p>هەر کاتێک پڕۆژەکە مۆدێلی داتای ئاڵۆز، پانێلی بەڕێوەبردنی ڕاستەقینە، یان چەند ئەپڵیکەیشنی پەیوەستی تێدابێت، جانگۆ کاتێکی زۆر پاشەکەوت دەکات.</p>
""".strip(),
                published_days_ago=2,
            ),
            dict(
                slug="rtl-design-systems",
                category=categories["design"],
                tags=["RTL", "طراحی", "دسترسی‌پذیری"],
                title_fa="طراحی برای راست‌به‌چپ یعنی بیشتر از آینه کردن یه رابط چپ‌به‌راست",
                title_ckb="دیزاینکردن بۆ ڕاست بۆ چەپ زیاترە لە ئاوێنەکردنەوەی ڕووکارێکی چەپ بۆ ڕاست",
                excerpt_fa="بیشتر تیم‌ها RTL رو با flip کردن CSS حل می‌کنن و بعد تعجب می‌کنن چرا هنوز به‌هم‌ریخته به نظر می‌رسه.",
                excerpt_ckb="زۆربەی تیمەکان RTL بە flipکردنی CSS چارەسەر دەکەن و پاشان سەرسام دەبن بۆچی هێشتا شێواوە.",
                body_fa="""
<p>وقتی اولین بار سایت رو برای فارسی و کردی سورانی آماده کردیم، اشتباه اول این بود که فکر کردیم کافیه <code>direction: rtl</code> رو ست کنیم و تمام. اما RTL واقعی خیلی عمیق‌تر از جهت متنه.</p>
<h2>چیزهایی که واقعاً باید عوض بشن</h2>
<ul>
<li>آیکون‌های جهت‌دار (فلش‌ها، chevron‌ها) باید mirror بشن، نه فقط جابه‌جا</li>
<li>padding و margin باید logical properties باشن (<code>margin-inline-start</code>) نه <code>margin-left</code></li>
<li>انیمیشن‌های اسلایدی باید جهتشون برعکس بشه</li>
<li>عکس‌های حاوی متن یا جهت (مثل یه دست که به سمتی اشاره می‌کنه) نیاز به بازبینی جدا دارن</li>
</ul>
<p>تجربه‌ی من: اگه از اول با logical properties بسازید، هزینه‌ی پشتیبانی از یه زبون دوم تقریباً صفره. اگه بعداً بخواید اضافه کنید، باید نصف CSS رو بازنویسی کنید.</p>
""".strip(),
                body_ckb="""
<p>یەکەم جار کە ماڵپەڕەکەمان بۆ فارسی و کوردیی سۆرانی ئامادە کرد، هەڵەی یەکەم ئەوە بوو کە وا بیر کردەوە بەسە <code>direction: rtl</code> دابنێین و تەواو. بەڵام RTL‌ی ڕاستەقینە زۆر قووڵترە لە ئاراستەی دەق.</p>
<h2>ئەو شتانەی کە ڕاستی پێویستە بگۆڕدرێن</h2>
<ul>
<li>ئایکۆنە ئاراستەدارەکان (تیرەکان، chevronەکان) پێویستە mirror بکرێن</li>
<li>padding و margin پێویستە logical properties بن</li>
<li>ئەنیمەیشنی سلایدی پێویستە ئاراستەکەی بگۆڕدرێت</li>
</ul>
""".strip(),
                published_days_ago=10,
            ),
            dict(
                slug="design-tokens-small-teams",
                category=categories["design"],
                tags=["Design Tokens", "Tailwind", "طراحی سیستم"],
                title_fa="دیزاین توکن برای تیم‌های یک‌نفره هم به‌درد می‌خوره",
                title_ckb="دیزاین تۆکن بۆ تیمی یەک‌کەسیش بەکەڵک دێت",
                excerpt_fa="فکر می‌کردم design token فقط برای تیم‌های بزرگ با چند برنده. بعد از یک بار تغییر رنگ برند در ده دقیقه، نظرم عوض شد.",
                excerpt_ckb="وا بیر دەکردمەوە design token تەنها بۆ تیمی گەورەیە. دوای یەک جار گۆڕینی ڕەنگی براند لە دە خولەکدا، بیرکردنەوەم گۆڕا.",
                body_fa="""
<p>تا قبل از این پروژه، فکر می‌کردم متغیرهای CSS برای دیزاین سیستم فقط زمانی به‌درد می‌خورن که چند دیزاینر و چند برند همزمان داری. اما وقتی مشتری تصمیم گرفت رنگ برند رو از قرمز به فیروزه‌ای تغییر بده، همه‌چیز عوض شد.</p>
<h2>ساختاری که استفاده کردیم</h2>
<p>به جای هاردکد کردن رنگ‌ها توی کلاس‌های Tailwind، همه‌چیز از <code>--brand</code> و <code>--accent</code> که توی دیتابیس قابل تنظیمن میاد. یعنی مدیر سایت می‌تونه بدون دست زدن به کد، کل ظاهر سایت رو عوض کنه.</p>
<p>نتیجه: یه تغییر رنگ که قبلاً نیم روز کار می‌برد، الان کمتر از یک دقیقه طول می‌کشه — از پنل مدیریت، بدون دیپلوی جدید.</p>
""".strip(),
                body_ckb="""
<p>پێش ئەم پڕۆژەیە، وا بیر دەکردمەوە گۆڕاوەکانی CSS بۆ سیستەمی دیزاین تەنها کاتێک بەکەڵک دێن کە چەند دیزاینەر هەبێت. بەڵام کاتێک کڕیار بڕیاری دا ڕەنگی براند بگۆڕێت، هەموو شتێک گۆڕا.</p>
<h2>پێکهاتەیەک کە بەکارمان هێنا</h2>
<p>لەبری هاردکۆدکردنی ڕەنگەکان، هەموو شتێک لە <code>--brand</code> و <code>--accent</code> دێت کە لە داتابەیسدا دەستکاریکراوە.</p>
""".strip(),
                published_days_ago=15,
            ),
            dict(
                slug="pricing-first-course",
                category=categories["business"],
                tags=["قیمت‌گذاری", "دوره آنلاین", "کسب‌وکار"],
                title_fa="چطور قیمت اولین دوره‌ی آنلاینم رو تعیین کردم (و چرا اشتباه بود)",
                title_ckb="چۆن نرخی یەکەم کۆرسی ئۆنلاینم دیاری کرد (و بۆچی هەڵە بوو)",
                excerpt_fa="قیمت اول رو بر اساس «چقدر پول لازم دارم» گذاشتم، نه بر اساس ارزشی که واقعاً ارائه می‌دادم. این هزینه‌اش رو داشت.",
                excerpt_ckb="نرخی یەکەمم لەسەر «چەند پارەم پێویستە» دانا، نەک لەسەر بەهایەک کە ڕاستی پێشکەشم دەکرد.",
                body_fa="""
<p>وقتی اولین دوره‌ی آموزشی رو منتشر کردم، قیمتش رو گذاشتم روی چیزی که فکر می‌کردم «برای شروع منطقیه». مشکل اینجا بود که این عدد از یه محاسبه‌ی شخصی اومده بود، نه از ارزش واقعی محتوا.</p>
<h2>سه چیزی که بعداً فهمیدم</h2>
<ul>
<li>قیمت پایین لزوماً باعث فروش بیشتر نمی‌شه — گاهی برعکس، اعتماد رو کم می‌کنه</li>
<li>مقایسه با رقبا مهم‌تر از هزینه‌ی خودمه</li>
<li>ارائه‌ی یه نسخه‌ی گروهی/سازمانی، میانگین درآمد رو بیشتر از افزایش قیمت پایه بالا می‌بره</li>
</ul>
<p>الان قیمت‌گذاری رو به عنوان بخشی از محصول می‌بینم، نه یه عدد که آخر کار می‌ذارم.</p>
""".strip(),
                body_ckb="""
<p>کاتێک یەکەم کۆرسی فێربوونم بڵاو کردەوە، نرخەکەم لەسەر ئەوە دانا کە وا بیر دەکردمەوە «بۆ دەستپێکردن لۆژیکییە». کێشەکە لێرەدا بوو کە ئەم ژمارەیە لە ژمێریارییەکی کەسییەوە هاتبوو.</p>
<h2>سێ شت کە دواتر تێگەیشتم</h2>
<ul>
<li>نرخی نزم بەرەڵا فرۆشی زیاتر نییە</li>
<li>بەراوردکردن لەگەڵ ڕکابەرەکان گرنگترە لە تێچووی خۆم</li>
</ul>
""".strip(),
                published_days_ago=21,
            ),
            dict(
                slug="freelance-to-product",
                category=categories["business"],
                tags=["فریلنس", "محصول", "کسب‌وکار"],
                title_fa="از فریلنسری به محصول: چیزی که هیچ‌کس درباره‌ی این انتقال بهم نگفت",
                title_ckb="لە فریلانسەوە بۆ بەرهەم: ئەوەی کەس پێی نەگوتم دەربارەی ئەم گواستنەوەیە",
                excerpt_fa="فریلنسری درآمد قابل پیش‌بینی می‌ده. ساختن محصول یعنی ماه‌ها بدون درآمد کار کردن روی چیزی که ممکنه هیچ‌وقت نفروشه.",
                excerpt_ckb="فریلانسەری داهاتێکی پێشبینیکراو دەدات. دروستکردنی بەرهەم واتە مانگان بەبێ داهات کارکردن.",
                body_fa="""
<p>بعد از پنج سال فریلنسری، تصمیم گرفتم اولین محصولم رو بسازم. چیزی که هیچ‌کس بهم نگفت این بود که سخت‌ترین بخش، فنی نیست — روانیه.</p>
<h2>تفاوت‌های واقعی</h2>
<p>توی فریلنسری، هر هفته یه چیز قابل‌تحویل داری و مشتری بهت فیدبک می‌ده. توی ساختن محصول، ممکنه ماه‌ها کار کنی بدون اینکه بدونی درست می‌ری یا نه.</p>
<blockquote>راهی که پیدا کردم: هر بخش از محصول رو مثل یه پروژه‌ی فریلنسری کوچیک با یه «مشتری فرضی» تصور کردم — این کمک کرد motivation رو نگه دارم.</blockquote>
<p>حالا، شش ماه بعد، دوره‌ها و محتوایی که ساختم دارن به کسایی که واقعاً باهاشون کار کردم می‌رسن — و این حس بهتری از هر پروژه‌ی فریلنسری‌ای بود.</p>
""".strip(),
                body_ckb="""
<p>دوای پێنج ساڵ فریلانسەری، بڕیارم دا یەکەم بەرهەمم دروست بکەم. ئەو شتەی کەس پێی نەگوتم ئەوە بوو کە قورسترین بەش، تەکنیکی نییە — دەروونییە.</p>
<h2>جیاوازییە ڕاستەقینەکان</h2>
<p>لە فریلانسەریدا، هەر هەفتە شتێکی گەیشتوو هەیە و کڕیار فیدباکت دەداتێ. لە دروستکردنی بەرهەمدا، لەوانەیە مانگان کار بکەیت بەبێ ئەوەی بزانیت بە ڕێگای ڕاست دەڕۆیت.</p>
""".strip(),
                published_days_ago=28,
            ),
        ]

        for index, spec in enumerate(posts):
            if Post.objects.filter(slug=spec["slug"]).exists():
                continue
            published_at = timezone.now() - timedelta(days=spec["published_days_ago"])
            post = Post(
                slug=spec["slug"],
                author=author,
                category=spec["category"],
                title_fa=spec["title_fa"],
                title_ckb=spec["title_ckb"],
                excerpt_fa=spec["excerpt_fa"],
                excerpt_ckb=spec["excerpt_ckb"],
                body_fa=spec["body_fa"],
                body_ckb=spec["body_ckb"],
                status=Post.STATUS_PUBLISHED,
                published_at=published_at,
            )
            post.cover_image.save(
                f"{spec['slug']}.png", ContentFile(make_gradient_png(index)), save=False
            )
            post.save()
            post.tags.add(*spec["tags"])

    def _seed_offerings(self):
        specs = [
            dict(
                slug="django-production-workshop",
                title_fa="کارگاه جنگو در محیط عملیاتی",
                title_ckb="کارگەی جانگۆ لە ژینگەی بەکارهێنان",
                summary_fa="از توسعه‌ی محلی تا دیپلوی امن — یک دوره‌ی فشرده‌ی عملی.",
                summary_ckb="لە گەشەپێدانی ناوخۆییەوە بۆ دابەزاندنی سەلامەت — کۆرسێکی فەشردەی کرداری.",
                body_fa="<p>این کارگاه برای کسایی طراحی شده که یک پروژه‌ی جنگو دارن و می‌خوان اون رو به شکل درست و امن روی سرور واقعی دیپلوی کنن.</p><ul><li>تنظیمات production در برابر development</li><li>rate limiting و احراز هویت دومرحله‌ای</li><li>لاگ‌گیری و مانیتورینگ خطا</li></ul>",
                body_ckb="<p>ئەم کارگەیە بۆ ئەو کەسانە دیزاین کراوە کە پڕۆژەیەکی جانگۆیان هەیە و دەیانەوێت بە شێوەیەکی دروست و سەلامەت لەسەر سێرڤەرێکی ڕاستەقینە دابەزێنن.</p>",
                price=Decimal("2900000"),
            ),
            dict(
                slug="ui-design-systems-course",
                title_fa="ساخت دیزاین سیستم از صفر",
                title_ckb="دروستکردنی سیستەمی دیزاین لە سفرەوە",
                summary_fa="توکن‌ها، کامپوننت‌ها، و مستندسازی — برای تیم‌های کوچیک و بزرگ.",
                summary_ckb="تۆکنەکان، کۆمپۆننتەکان، و بەڵگەنامەکردن — بۆ تیمی بچووک و گەورە.",
                body_fa="<p>یاد می‌گیرید چطور یه دیزاین سیستم قابل نگهداری بسازید که با رشد تیمتون هم مقیاس‌پذیر بمونه.</p>",
                body_ckb="<p>فێردەبیت چۆن سیستەمی دیزاینێکی پاراستنی دروست بکەیت کە لەگەڵ گەشەی تیمەکەت هاوسەنگ بمێنێتەوە.</p>",
                price=None,
            ),
            dict(
                slug="freelance-to-saas",
                title_fa="از فریلنسری به SaaS",
                title_ckb="لە فریلانسەوە بۆ SaaS",
                summary_fa="یک نقشه‌ی راه عملی برای ساختن اولین محصول درآمدزات.",
                summary_ckb="نەخشەڕێگایەکی کرداری بۆ دروستکردنی یەکەم بەرهەمی داهاتدارت.",
                body_fa="<p>از ایده تا اولین مشتری پرداخت‌کننده — بدون نیاز به سرمایه‌گذار یا تیم بزرگ.</p>",
                body_ckb="<p>لە بیرۆکەوە بۆ یەکەم کڕیاری پارەدەر — بەبێ پێویستی بە وەبەرهێنەر یان تیمی گەورە.</p>",
                price=Decimal("4500000"),
            ),
        ]
        offerings = []
        for index, spec in enumerate(specs):
            offering, created = Offering.objects.get_or_create(
                slug=spec["slug"],
                defaults=dict(
                    title_fa=spec["title_fa"],
                    title_ckb=spec["title_ckb"],
                    summary_fa=spec["summary_fa"],
                    summary_ckb=spec["summary_ckb"],
                    body_fa=spec["body_fa"],
                    body_ckb=spec["body_ckb"],
                    price=spec["price"],
                    status=Offering.STATUS_PUBLISHED,
                ),
            )
            if created:
                offering.cover_image.save(
                    f"{spec['slug']}.png", ContentFile(make_gradient_png(index + 3)), save=True
                )
                for weeks_ahead in (2, 6):
                    Session.objects.create(
                        offering=offering,
                        starts_at=timezone.now() + timedelta(weeks=weeks_ahead),
                        ends_at=timezone.now() + timedelta(weeks=weeks_ahead, hours=3),
                        location="آنلاین — Google Meet",
                        capacity=20,
                    )
            offerings.append(offering)
        return offerings

    def _seed_testimonials(self, offerings):
        # author_role/quote are modeltranslation fields (author_role_fa/_ckb,
        # quote_fa/_ckb) — set both explicitly, never the bare `author_role=`/
        # `quote=` kwarg, which would only ever write whichever language is
        # active and silently leave the other blank (the same class of bug
        # fixed in Post/Offering/FlatPage.save() after the round-2 audit).
        specs = [
            dict(
                name="سارا احمدی",
                role_fa="توسعه‌دهنده فرانت‌اند", role_ckb="گەشەپێدەری فرەنتئێند",
                quote_fa="دوره‌ی جنگو دقیقاً همون چیزی بود که برای دیپلوی کردن پروژه‌ی واقعیم لازم داشتم. توضیحات ساده و بدون حاشیه.",
                quote_ckb="کۆرسی جانگۆ ڕاستەوخۆ ئەو شتە بوو کە بۆ دابەزاندنی پڕۆژە ڕاستەقینەکەم پێویستم بوو.",
                offering_idx=0,
            ),
            dict(
                name="هێمن کەریمی",
                role_fa="مدیر محصول", role_ckb="بەڕێوەبەری بەرهەم",
                quote_fa="کارگاه دیزاین سیستم دیدم رو نسبت به کامپوننت‌ها کاملاً عوض کرد. خیلی ممنونم.",
                quote_ckb="کارگەی سیستەمی دیزاین بۆچوونم بۆ کۆمپۆننتەکان بە تەواوی گۆڕی. زۆر سوپاس.",
                offering_idx=1,
            ),
            dict(
                name="نیلوفر رضایی",
                role_fa="بنیان‌گذار استارتاپ", role_ckb="دامەزرێنەری startup",
                quote_fa="قبل از این دوره نمی‌دونستم قیمت‌گذاری این‌قدر استراتژیکه. توصیه می‌کنم به هر کسی که محصول می‌سازه.",
                quote_ckb="پێش ئەم کۆرسە نەمدەزانی نرخنان ئەوەندە ستراتیژییە. پێشنیاری دەکەم بۆ هەرکەسێک بەرهەم دروست دەکات.",
                offering_idx=2,
            ),
            dict(
                name="آرمان توکلی",
                role_fa="برنامه‌نویس بک‌اند", role_ckb="پڕۆگرامەری باکئێند",
                quote_fa="مثال‌های واقعی از پروژه‌های production بودن، نه فقط تئوری. همین باعث شد واقعاً یاد بگیرم.",
                quote_ckb="نموونەکان لە پڕۆژەی ڕاستەقینەی production بوون، نەک تەنها تیۆری. ئەمە وایکرد ڕاستی فێربم.",
                offering_idx=None,
            ),
            dict(
                name="ڕۆژین محەمەدی",
                role_fa="طراح UX", role_ckb="دیزاینەری UX",
                quote_fa="مستقیم توی پروژه‌هام ازش استفاده کردم. نتیجه‌اش خیلی بهتر از چیزی بود که انتظار داشتم.",
                quote_ckb="ڕاستەوخۆ لە پڕۆژەکانمدا بەکارمهێنا. ئەنجامەکە زۆر باشتر بوو لەوەی چاوەڕێم دەکرد.",
                offering_idx=None,
            ),
        ]
        for order, spec in enumerate(specs):
            offering = offerings[spec["offering_idx"]] if spec["offering_idx"] is not None else None
            Testimonial.objects.get_or_create(
                author_name=spec["name"],
                defaults=dict(
                    author_role_fa=spec["role_fa"],
                    author_role_ckb=spec["role_ckb"],
                    quote_fa=spec["quote_fa"],
                    quote_ckb=spec["quote_ckb"],
                    offering=offering,
                    is_approved=True,
                    order=order,
                ),
            )

    def _seed_leads(self):
        specs = [
            dict(
                slug="django-deploy-checklist",
                title_fa="چک‌لیست دیپلوی امن جنگو",
                title_ckb="چێکلیستی دابەزاندنی سەلامەتی جانگۆ",
                description_fa="یک راهنمای PDF کوتاه با ۲۰ آیتم قبل از هر دیپلوی به production.",
                description_ckb="ڕێبەرێکی PDF کورت بە ٢٠ بڕگە پێش هەر دابەزاندنێک بۆ production.",
            ),
            dict(
                slug="pricing-worksheet",
                title_fa="کاربرگ قیمت‌گذاری دوره‌ی آنلاین",
                title_ckb="وەرەقەی کاری نرخنانی کۆرسی ئۆنلاین",
                description_fa="یک قالب ساده برای محاسبه‌ی قیمت واقعی دوره‌تون بر اساس ارزش، نه حدس.",
                description_ckb="داڕشتەیەکی سادە بۆ ژمێریاری نرخی ڕاستەقینەی کۆرسەکەت.",
            ),
        ]
        for index, spec in enumerate(specs):
            if LeadMagnet.objects.filter(slug=spec["slug"]).exists():
                continue
            lead = LeadMagnet(
                slug=spec["slug"],
                title_fa=spec["title_fa"],
                title_ckb=spec["title_ckb"],
                description_fa=spec["description_fa"],
                description_ckb=spec["description_ckb"],
                status=LeadMagnet.STATUS_PUBLISHED,
            )
            lead.cover_image.save(
                f"{spec['slug']}.png", ContentFile(make_gradient_png(index + 6)), save=False
            )
            lead.file.save(
                f"{spec['slug']}.pdf", ContentFile(make_fake_pdf(spec["title_fa"])), save=False
            )
            lead.save()

    def _seed_pages(self):
        pages = [
            dict(
                slug="about",
                title_fa="درباره ما",
                title_ckb="دەربارەمان",
                body_fa="""
<p>سلام، من آرش هستم — توسعه‌دهنده و مدرس، با علاقه‌ی خاص به جنگو، طراحی سیستم و ساختن محصولاتی که واقعاً استفاده می‌شن.</p>
<p>این وبلاگ جاییه که تجربه‌های واقعی از پروژه‌ها، دوره‌ها و اشتباه‌هایی که ازشون یاد گرفتم رو به فارسی و کردی سورانی می‌نویسم — بدون واسطه و بدون تئوری خشک.</p>
""".strip(),
                body_ckb="""
<p>سڵاو، من ئاراشم — گەشەپێدەر و مامۆستا، بە حەزێکی تایبەت بۆ جانگۆ، دیزاینی سیستەم و دروستکردنی بەرهەمی کە ڕاستی بەکاردێت.</p>
<p>ئەم بلۆگە شوێنێکە کە ئەزموونی ڕاستەقینەم لە پڕۆژە، کۆرس و هەڵانەی کە لێیان فێربووم بە فارسی و کوردیی سۆرانی دەنووسم.</p>
""".strip(),
            ),
            dict(
                slug="contact",
                title_fa="تماس با ما",
                title_ckb="پەیوەندیمان پێوە بکە",
                body_fa="<p>سوالی دارید یا می‌خواید درباره‌ی یه دوره یا همکاری صحبت کنیم؟ فرم زیر رو پر کنید یا مستقیم ایمیل بزنید.</p>",
                body_ckb="<p>پرسیارێکت هەیە یان دەتەوێت دەربارەی کۆرسێک یان هاوکاری قسە بکەین؟ فۆرمی خوارەوە پڕ بکەوە یان ڕاستەوخۆ ئیمەیل بنێرە.</p>",
            ),
        ]
        for spec in pages:
            FlatPage.objects.get_or_create(
                slug=spec["slug"],
                defaults=dict(
                    title_fa=spec["title_fa"],
                    title_ckb=spec["title_ckb"],
                    body_fa=spec["body_fa"],
                    body_ckb=spec["body_ckb"],
                ),
            )

    def _publish_sections(self):
        NavItem.objects.filter(
            url_name__in=["offerings:list", "testimonials:list", "leads:list", "pages:about"]
        ).update(is_visible=True)
