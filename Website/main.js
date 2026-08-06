import './style.css'

// Translations Dictionary
const translations = {
  tr: {
    nav_home: "Ana Sayfa",
    nav_features: "Özellikler",
    nav_versions: "Sürümler",
    nav_news: "Haberler",
    hero_title_1: "Oyun Deneyiminizi <br/>",
    hero_title_2: "Mükemmelleştirin.",
    hero_subtitle: "1.21.11 Fabric altyapısıyla yep yeni Pierre Client sürümü artık erişilebilir. Yüksek FPS, entegre modlar ve ultra güvenlik.",
    hero_download: "HEMEN İNDİR",
    hero_more: "Daha Fazla Bilgi",
    hero_version: "Sürüm: v1.0.0 | Boyut: ~160 MB",
    feat_title_1: "Neden",
    feat_1_title: "Yüksek Performans",
    feat_1_desc: "Sodium, Lithium ve diğer optimizasyon modlarıyla standart istemcilere göre çok daha yüksek FPS değerleri.",
    feat_2_title: "Modern Arayüz",
    feat_2_desc: "Oyuncular için özel olarak tasarlanmış, cam tasarımlı ve tamamen kişiselleştirilebilir Launcher deneyimi.",
    feat_3_title: "Bağımsız Kurulum",
    feat_3_desc: "Bilgisayarınızda Java yüklü olmasına bile gerek yok. Kurulum aracı tüm kütüphaneleri arka planda kendi ayarlar.",
    news_title_1: "Canlı",
    news_title_2: "Haberler",
    news_subtitle: "Oyun içi güncellemeler ve duyurular doğrudan buraya yansır.",
    news_loading: "Haberler yükleniyor...",
    news_empty: "Şu an gösterilecek bir haber bulunmuyor.",
    news_error: "Haberler yüklenirken bir sorun oluştu. Daha sonra tekrar deneyin.",
    com_title: "Topluluğa Katılın",
    com_desc: "Binlerce oyuncunun bulunduğu açık kaynak topluluğumuza katılın, yeni güncellemeleri ilk siz öğrenin.",
    com_btn: "Github'da Yıldızla",
    faq_title_1: "Sıkça Sorulan",
    faq_title_2: "Sorular",
    faq_1_q: "Tamamen ücretsiz mi?",
    faq_1_a: "Evet, Pierre Client %100 açık kaynak kodlu ve tamamen ücretsizdir.",
    faq_2_q: "Hangi Minecraft sürümünü destekliyor?",
    faq_2_a: "Şu anda en popüler ve güncel sürüm olan 1.21.11 Fabric altyapısı ile tam uyumlu çalışmaktadır.",
    faq_3_q: "Java kurmama gerek var mı?",
    faq_3_a: "Hayır! İndireceğiniz Kurulum (.exe) dosyası oyunun çalışması için gereken Java ve diğer tüm altyapıları arka planda kendisi halleder.",
    dl_title_1: "İndirme",
    dl_title_2: "Merkezi",
    dl_subtitle: "Lütfen Pierre Client indirmek için bir kaynak seçin.",
    dl_direct_title: "Direkt İndir (Github)",
    dl_direct_desc: "Resmi Github sunucularımız üzerinden en güncel ve güvenli sürümü hızlıca indirin.",
    dl_btn_download: "Şimdi İndir",
    dl_drive_title: "Google Drive",
    dl_drive_desc: "Google altyapısı kullanarak dosyayı indirin. Kota sınırlarına takılabilir.",
    dl_btn_soon: "Çok Yakında",
    ver_title_1: "Tüm",
    ver_title_2: "Sürümler",
    ver_subtitle: "Pierre Client'ın yayınlanmış tüm eski ve yeni sürümleri.",
    ver_latest: "En Yeni",
    ver_btn_download: "İndir"
  },
  en: {
    nav_home: "Home",
    nav_features: "Features",
    nav_versions: "Versions",
    nav_news: "News",
    hero_title_1: "Perfect Your <br/>",
    hero_title_2: "Gaming Experience.",
    hero_subtitle: "The brand new Pierre Client with 1.21.11 Fabric is now available. High FPS, integrated mods, and ultra security.",
    hero_download: "DOWNLOAD NOW",
    hero_more: "Learn More",
    hero_version: "Version: v1.0.0 | Size: ~160 MB",
    feat_title_1: "Why",
    feat_1_title: "High Performance",
    feat_1_desc: "Much higher FPS values compared to standard clients with Sodium, Lithium, and other optimization mods.",
    feat_2_title: "Modern Interface",
    feat_2_desc: "A custom-designed, glassmorphic, and fully customizable Launcher experience for players.",
    feat_3_title: "Standalone Install",
    feat_3_desc: "You don't even need Java installed. The installer automatically handles all libraries in the background.",
    news_title_1: "Live",
    news_title_2: "News",
    news_subtitle: "In-game updates and announcements reflect directly here.",
    news_loading: "Loading news...",
    news_empty: "There is no news to show right now.",
    news_error: "An error occurred while loading news. Please try again later.",
    com_title: "Join the Community",
    com_desc: "Join our open-source community of thousands of players and be the first to know about new updates.",
    com_btn: "Star on Github",
    faq_title_1: "Frequently Asked",
    faq_title_2: "Questions",
    faq_1_q: "Is it completely free?",
    faq_1_a: "Yes, Pierre Client is 100% open source and completely free.",
    faq_2_q: "Which Minecraft version does it support?",
    faq_2_a: "It currently runs fully compatible with the most popular and up-to-date version, 1.21.11 Fabric infrastructure.",
    faq_3_q: "Do I need to install Java?",
    faq_3_a: "No! The Setup (.exe) file you download will handle Java and all other required infrastructures in the background on its own.",
    dl_title_1: "Download",
    dl_title_2: "Center",
    dl_subtitle: "Please select a source to download Pierre Client.",
    dl_direct_title: "Direct Download (Github)",
    dl_direct_desc: "Download the most up-to-date and secure version quickly via our official Github servers.",
    dl_btn_download: "Download Now",
    dl_drive_title: "Google Drive",
    dl_drive_desc: "Download the file using Google infrastructure. May be subject to quota limits.",
    dl_btn_soon: "Coming Soon",
    ver_title_1: "All",
    ver_title_2: "Versions",
    ver_subtitle: "All past and present published versions of Pierre Client.",
    ver_latest: "Latest",
    ver_btn_download: "Download"
  }
};

let currentLang = 'tr';
let newsDataCache = []; // Store news so we can re-render when language changes

document.addEventListener('DOMContentLoaded', async () => {
  // --- 1. Language Support ---
  const langBtn = document.getElementById('lang-btn');
  
  const updateLanguage = () => {
    document.querySelectorAll('[data-i18n]').forEach(el => {
      const key = el.getAttribute('data-i18n');
      if (translations[currentLang][key]) {
        // If it contains HTML (like <br/>), use innerHTML, else textContent
        if (translations[currentLang][key].includes('<br/>')) {
          el.innerHTML = translations[currentLang][key];
        } else {
          el.textContent = translations[currentLang][key];
        }
      }
    });
    
    // Re-render news if loaded
    renderNews();
  };

  langBtn.addEventListener('click', () => {
    currentLang = currentLang === 'tr' ? 'en' : 'tr';
    updateLanguage();
  });

  // --- 2. SPA Routing (Views) ---
  const navLinks = document.querySelectorAll('.nav-link');
  const views = document.querySelectorAll('.view');

  navLinks.forEach(link => {
    link.addEventListener('click', (e) => {
      e.preventDefault();
      
      const targetView = link.getAttribute('data-target');
      
      // Update active nav link
      document.querySelectorAll('.nav-link').forEach(n => n.classList.remove('active'));
      // Only set nav items in header as active, not buttons in hero
      if(link.closest('nav')) link.classList.add('active');

      // Hide all views, show target
      views.forEach(view => {
        if (view.id === 'view-' + targetView) {
          view.classList.add('active');
        } else {
          view.classList.remove('active');
        }
      });
    });
  });

  // --- 3. Dynamic News ---
  const newsContainer = document.getElementById('news-container');
  
  const renderNews = () => {
    if (newsDataCache.length === 0) return;
    
    newsContainer.innerHTML = '';
    newsDataCache.forEach(item => {
      const card = document.createElement('div');
      card.className = 'news-card glass-panel';
      
      const title = currentLang === 'en' ? item.titleEn : item.titleTr;
      const desc = currentLang === 'en' ? item.descEn : item.descTr;
      const date = currentLang === 'en' ? item.dateEn : item.dateTr;
      
      card.innerHTML = `
        <h4 class="news-title">${title}</h4>
        <p class="news-desc">${desc}</p>
        <span class="news-date">${date}</span>
      `;
      
      newsContainer.appendChild(card);
    });
  };

  try {
    const response = await fetch('https://raw.githubusercontent.com/aliisinne/PierreClient/master/news.json');
    if (!response.ok) throw new Error('Network response was not ok');
    
    newsDataCache = await response.json();
    
    if (newsDataCache && newsDataCache.length > 0) {
      renderNews();
    } else {
      newsContainer.innerHTML = `<div class="loading" data-i18n="news_empty">${translations[currentLang].news_empty}</div>`;
    }
  } catch (error) {
    console.error('Error fetching news:', error);
    newsContainer.innerHTML = `<div class="loading" data-i18n="news_error">${translations[currentLang].news_error}</div>`;
  }
  
  // Initialize language on load
  updateLanguage();
});
