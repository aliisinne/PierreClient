import './style.css'

document.addEventListener('DOMContentLoaded', async () => {
  const newsContainer = document.getElementById('news-container');
  
  try {
    // Fetch news from the raw github url
    // To ensure Cloudflare always has the latest, we fetch client-side from the master branch!
    const response = await fetch('https://raw.githubusercontent.com/aliisinne/PierreClient/master/news.json');
    
    if (!response.ok) throw new Error('Network response was not ok');
    
    const newsData = await response.json();
    
    // Clear loading text
    newsContainer.innerHTML = '';
    
    // Parse and display news
    if (newsData && newsData.length > 0) {
      newsData.forEach(item => {
        const card = document.createElement('div');
        card.className = 'news-card glass-panel';
        
        card.innerHTML = `
          <h4 class="news-title">${item.titleTr}</h4>
          <p class="news-desc">${item.descTr}</p>
          <span class="news-date">${item.dateTr}</span>
        `;
        
        newsContainer.appendChild(card);
      });
    } else {
      newsContainer.innerHTML = '<div class="loading">Şu an gösterilecek bir haber bulunmuyor.</div>';
    }
    
  } catch (error) {
    console.error('Haberler yüklenirken hata oluştu:', error);
    newsContainer.innerHTML = '<div class="loading">Haberler yüklenirken bir sorun oluştu. Daha sonra tekrar deneyin.</div>';
  }
  
  // Smooth scrolling for navigation links
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
      e.preventDefault();
      document.querySelector(this.getAttribute('href')).scrollIntoView({
        behavior: 'smooth'
      });
    });
  });
});
