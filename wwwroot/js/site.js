document.addEventListener('DOMContentLoaded', function() {
    initializePageFeatures();
    initializeScrollAnimations();
    initializeButtonEffects();
    initializePage();
});

function initializePageFeatures() {
    const currentPage = window.location.pathname;
    
    if (currentPage.includes('Index')) {
        initializeIndexPage();
    } else if (currentPage.includes('Players')) {
        initializePlayersPage();
    } else if (currentPage.includes('About')) {
        initializeAboutPage();
    }
}

// Initialize scroll-triggered animations using Intersection Observer
function initializeScrollAnimations() {
    const animatedElements = document.querySelectorAll('.stats-card, .feature-card, .dev-card, .tech-card, .quick-actions .card');
    
    if (animatedElements.length === 0) return;
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animationPlayState = 'running';
                entry.target.classList.add('animate-in');
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '-50px'
    });
    
    animatedElements.forEach(element => {
        element.style.animationPlayState = 'paused';
        observer.observe(element);
    });
}

// Enhanced button hover effects without ripple
function initializeButtonEffects() {
    const buttons = document.querySelectorAll('.btn, .card, .stats-card, .feature-card');
    
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px)';
        });
        
        button.addEventListener('mouseleave', function() {
            this.style.transform = '';
        });
    });
}

function initializePage() {
    const style = document.createElement('style');
    style.textContent = `
        .animate-in {
            animation-delay: 0s !important;
        }
        
        .loading-state {
            pointer-events: none;
            opacity: 0.7;
            position: relative;
        }
        
        .loading-state::after {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            width: 20px;
            height: 20px;
            margin: -10px 0 0 -10px;
            border: 2px solid transparent;
            border-top-color: #4299E1;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            z-index: 1000;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        .tooltip {
            position: absolute;
            background: rgba(0, 0, 0, 0.8);
            color: white;
            padding: 0.5rem 1rem;
            border-radius: 8px;
            font-size: 0.9rem;
            z-index: 1000;
            pointer-events: none;
            opacity: 0;
            transition: opacity 0.3s ease;
            backdrop-filter: blur(10px);
        }
        
        .tooltip.show {
            opacity: 1;
        }
        
        .tooltip::before {
            content: '';
            position: absolute;
            top: 100%;
            left: 50%;
            transform: translateX(-50%);
            border: 5px solid transparent;
            border-top-color: rgba(0, 0, 0, 0.8);
        }
    `;
    document.head.appendChild(style);
    
    initializeTooltips();
}

function initializeIndexPage() {
    const statsNumbers = document.querySelectorAll('.stats-number');
    initializeCounters(statsNumbers);
}

// Animate counters from 0 to their final values
function initializeCounters(elements) {
    elements.forEach(element => {
        const finalValue = parseInt(element.textContent);
        const duration = 2000;
        const startTime = performance.now();
        
        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            const currentValue = Math.floor(progress * finalValue);
            
            element.textContent = currentValue.toLocaleString();
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };
        
        requestAnimationFrame(animate);
    });
}

function initializePlayersPage() {
    const searchInput = document.querySelector('.search-input input');
    const clearButton = document.querySelector('.clear-search');
    const table = document.querySelector('.table tbody');
    
    if (searchInput && clearButton && table) {
        initializeSearch(searchInput, clearButton, table);
        initializeTableSorting();
        initializeKeyboardShortcuts(searchInput);
    }
}

// Initialize live search functionality for players
function initializeSearch(input, clearButton, table) {
    const originalRows = Array.from(table.querySelectorAll('tr'));
    
    input.addEventListener('input', function() {
        const searchTerm = this.value.toLowerCase();
        
        if (searchTerm) {
            clearButton.classList.add('visible');
        } else {
            clearButton.classList.remove('visible');
        }
        
        originalRows.forEach(row => {
            const playerName = row.querySelector('.player-details h6');
            const playerEmail = row.querySelector('.player-details small');
            
            if (playerName && playerEmail) {
                const nameMatch = playerName.textContent.toLowerCase().includes(searchTerm);
                const emailMatch = playerEmail.textContent.toLowerCase().includes(searchTerm);
                
                if (nameMatch || emailMatch || searchTerm === '') {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            }
        });
        
        const visibleRows = originalRows.filter(row => row.style.display !== 'none');
        if (visibleRows.length === 0 && searchTerm) {
            showNoResults(table);
        } else {
            hideNoResults(table);
        }
    });
    
    clearButton.addEventListener('click', function() {
        input.value = '';
        input.dispatchEvent(new Event('input'));
        input.focus();
    });
}

// Initialize table sorting functionality
function initializeTableSorting() {
    const headers = document.querySelectorAll('.table th.sortable');
    
    headers.forEach(header => {
        header.addEventListener('click', function() {
            const table = this.closest('table');
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr'));
            const columnIndex = Array.from(this.parentElement.children).indexOf(this);
            const isAsc = this.classList.contains('sorted-asc');
            
            headers.forEach(h => h.classList.remove('sorted-asc', 'sorted-desc'));
            
            if (isAsc) {
                this.classList.add('sorted-desc');
            } else {
                this.classList.add('sorted-asc');
            }
            
            rows.sort((a, b) => {
                const aText = a.children[columnIndex].textContent.trim();
                const bText = b.children[columnIndex].textContent.trim();
                
                const aNum = parseFloat(aText.replace(/[^\d.-]/g, ''));
                const bNum = parseFloat(bText.replace(/[^\d.-]/g, ''));
                
                let comparison = 0;
                if (!isNaN(aNum) && !isNaN(bNum)) {
                    comparison = aNum - bNum;
                } else {
                    comparison = aText.localeCompare(bText);
                }
                
                return isAsc ? -comparison : comparison;
            });
            
            rows.forEach(row => tbody.appendChild(row));
        });
    });
}

// Initialize keyboard shortcuts for power users
function initializeKeyboardShortcuts(searchInput) {
    document.addEventListener('keydown', function(e) {
        if (e.ctrlKey && e.key === 'f') {
            e.preventDefault();
            searchInput.focus();
        }
        
        if (e.key === 'Escape' && document.activeElement === searchInput) {
            searchInput.blur();
            if (searchInput.value) {
                searchInput.value = '';
                searchInput.dispatchEvent(new Event('input'));
            }
        }
    });
}

function showNoResults(table) {
    let noResultsRow = table.querySelector('.no-results-row');
    if (!noResultsRow) {
        noResultsRow = document.createElement('tr');
        noResultsRow.className = 'no-results-row';
        noResultsRow.innerHTML = `
            <td colspan="100%" class="no-results">
                No players found matching your search criteria.
            </td>
        `;
        table.appendChild(noResultsRow);
    }
}

function hideNoResults(table) {
    const noResultsRow = table.querySelector('.no-results-row');
    if (noResultsRow) {
        noResultsRow.remove();
    }
}

function initializeAboutPage() {
    const devNames = document.querySelectorAll('.dev-info h3');
    initializeTypingEffect(devNames);
}

// Create typing effect for developer names
function initializeTypingEffect(elements) {
    elements.forEach((element, index) => {
        const text = element.textContent;
        element.textContent = '';
        element.style.borderRight = '2px solid #4299E1';
        element.style.animation = 'none';
        
        setTimeout(() => {
            let charIndex = 0;
            const typeInterval = setInterval(() => {
                element.textContent += text[charIndex];
                charIndex++;
                
                if (charIndex >= text.length) {
                    clearInterval(typeInterval);
                    setTimeout(() => {
                        element.style.borderRight = 'none';
                    }, 500);
                }
            }, 100);
        }, index * 500);
    });
}

// Initialize comprehensive tooltip system
function initializeTooltips() {
    const tooltipElements = document.querySelectorAll('[data-tooltip]');
    
    tooltipElements.forEach(element => {
        let tooltip = null;
        
        element.addEventListener('mouseenter', function() {
            const text = this.getAttribute('data-tooltip');
            if (!text) return;
            
            tooltip = document.createElement('div');
            tooltip.className = 'tooltip';
            tooltip.textContent = text;
            document.body.appendChild(tooltip);
            
            const rect = this.getBoundingClientRect();
            tooltip.style.left = rect.left + rect.width / 2 - tooltip.offsetWidth / 2 + 'px';
            tooltip.style.top = rect.top - tooltip.offsetHeight - 10 + 'px';
            
            setTimeout(() => {
                if (tooltip) tooltip.classList.add('show');
            }, 100);
        });
        
        element.addEventListener('mouseleave', function() {
            if (tooltip) {
                tooltip.classList.remove('show');
                setTimeout(() => {
                    if (tooltip && tooltip.parentNode) {
                        tooltip.parentNode.removeChild(tooltip);
                    }
                }, 300);
            }
        });
    });
}

function addLoadingState(element) {
    element.classList.add('loading-state');
}

function removeLoadingState(element) {
    element.classList.remove('loading-state');
}

window.ConnectFourUI = {
    addLoadingState,
    removeLoadingState,
    initializeCounters,
    initializeTooltips
}; 