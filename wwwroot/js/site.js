// Web 2.0 İnteraktif Şube - Ana JavaScript Dosyası

// TCKN Maskeleme
function maskTCKN(input) {
    input.addEventListener('input', function(e) {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 11) value = value.substring(0, 11);
        
        if (value.length > 0) {
            value = value.substring(0, 3) + '***' + value.substring(6);
        }
        
        e.target.value = value;
    });
}

// Telefon Maskeleme
function maskPhone(input) {
    input.addEventListener('input', function(e) {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 10) value = value.substring(0, 10);
        
        if (value.length > 0) {
            value = '5' + value.substring(1, 4) + '***' + value.substring(7);
        }
        
        e.target.value = value;
    });
}

// PIN Maskeleme (Sadece rakamlar)
function maskPIN(input) {
    input.addEventListener('input', function(e) {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 6) value = value.substring(0, 6);
        e.target.value = value;
    });
    
    // Nümerik klavye için
    input.setAttribute('inputmode', 'numeric');
    input.setAttribute('pattern', '[0-9]*');
}

// Toast Bildirim Sistemi
function showToast(message, type = 'success') {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) return;
    
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <div class="d-flex justify-content-between align-items-center">
            <span>
                <i class="fas ${type === 'success' ? 'fa-check-circle' : type === 'error' ? 'fa-exclamation-circle' : 'fa-exclamation-triangle'} me-2"></i>
                ${message}
            </span>
            <button type="button" class="btn-close" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    // 5 saniye sonra otomatik kaldır
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

// Form Validasyonu
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return false;
    
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('error');
            isValid = false;
        } else {
            input.classList.remove('error');
        }
    });
    
    return isValid;
}

// TCKN Validasyonu
function validateTCKN(tckn) {
    const cleanTCKN = tckn.replace(/\D/g, '');
    if (cleanTCKN.length !== 11) return false;
    if (cleanTCKN[0] === '0') return false;
    
    // Basit kontrol (gerçek uygulamada daha detaylı olmalı)
    return /^\d{11}$/.test(cleanTCKN);
}

// Telefon Validasyonu
function validatePhone(phone) {
    const cleanPhone = phone.replace(/\D/g, '');
    return cleanPhone.length === 10 && cleanPhone.startsWith('5');
}

// PIN Validasyonu
function validatePIN(pin) {
    return /^\d{6}$/.test(pin);
}

// Loading Göstergesi
function showLoading(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.innerHTML = '<div class="spinner"></div>';
    }
}

// Sayı Formatlama (Para formatı)
function formatCurrency(amount) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(amount);
}

// Tarih Formatlama
function formatDate(date) {
    return new Intl.DateTimeFormat('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// Modal backdrop ve overlay temizleme
function cleanupModals() {
    // Tüm backdrop ve overlay'leri kaldır
    const backdrops = document.querySelectorAll('.modal-backdrop, .offcanvas-backdrop, [class*="backdrop"], [class*="overlay"]');
    backdrops.forEach(backdrop => {
        backdrop.remove();
        backdrop.style.display = 'none';
        backdrop.style.visibility = 'hidden';
        backdrop.style.pointerEvents = 'none';
    });
    
    // Body'den modal-open class'ını kaldır
    document.body.classList.remove('modal-open');
    
    // Body'nin overflow ve padding stillerini sıfırla
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';
    document.body.style.position = '';
    
    // Body üzerindeki tüm overlay stillerini kaldır
    document.body.style.pointerEvents = '';
    
    // Tüm elementlerin pointer-events'ini kontrol et
    const allElements = document.querySelectorAll('*');
    allElements.forEach(el => {
        const computedStyle = window.getComputedStyle(el);
        if (computedStyle.position === 'fixed' && 
            (computedStyle.backgroundColor === 'rgba(0, 0, 0, 0.5)' || 
             computedStyle.backgroundColor === 'rgb(0, 0, 0)' ||
             el.classList.contains('backdrop') ||
             el.classList.contains('overlay'))) {
            el.style.display = 'none';
            el.style.pointerEvents = 'none';
        }
    });
}

// Sayfa yüklendiğinde
document.addEventListener('DOMContentLoaded', function() {
    // Modal backdrop temizleme
    cleanupModals();
    
    // TCKN inputları için maskeleme
    const tcknInputs = document.querySelectorAll('input[data-mask="tckn"]');
    tcknInputs.forEach(input => maskTCKN(input));
    
    // Telefon inputları için maskeleme
    const phoneInputs = document.querySelectorAll('input[data-mask="phone"]');
    phoneInputs.forEach(input => maskPhone(input));
    
    // PIN inputları için maskeleme
    const pinInputs = document.querySelectorAll('input[data-mask="pin"]');
    pinInputs.forEach(input => maskPIN(input));
    
    // Form validasyonu
    const forms = document.querySelectorAll('form[data-validate="true"]');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!validateForm(form.id)) {
                e.preventDefault();
                showToast('Lütfen tüm zorunlu alanları doldurun.', 'error');
            }
        });
    });
    
    // Sayfa görünür olduğunda da temizle
    document.addEventListener('visibilitychange', function() {
        if (!document.hidden) {
            cleanupModals();
        }
    });
    
    // Her 2 saniyede bir kontrol et (güvenlik için)
    setInterval(cleanupModals, 2000);
});

// Animasyonlar için Intersection Observer
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver(function(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0)';
        }
    });
}, observerOptions);

// Fade-in animasyonu için
document.addEventListener('DOMContentLoaded', function() {
    const animatedElements = document.querySelectorAll('.fade-in');
    animatedElements.forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });
});
