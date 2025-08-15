<template>
<aside class="fixed bg-black/50 right-4 top-1/2 -translate-y-1/2 z-50 w-[18rem] max-w-[90vw] text-white rounded-2xl" role="complementary" aria-label="Outfit Auswahl Menü" v-if="menuShow">
    <div class="rounded-2xl shadow-xl border p-4 flex flex-col gap-3">
        <h2 class="text-lg font-semibold tracking-tight" style="font-size: 1.5rem">{{ title }}</h2>
        <hr />
        <label class="text-sm font-medium">Category</label>
        <div class="relative select-none">
            <div class="w-full px-3 py-2 rounded-xl bg-black/70 text-white text-left cursor-pointer" @click="openDropdown('category')">
                <span class="truncate">{{ categoryLabel }}</span>
            </div>
            <ul v-if="open.category" class="absolute left-0 right-0 mt-1 max-h-40 overflow-auto rounded-xl border border-white/10 bg-black/70 text-white shadow-2xl z-[9999] no-scrollbar" role="listbox">
                <li class="p-2">
                    <input type="text" v-model="search.category" placeholder="Suchen..." class="w-full px-2 py-1 rounded bg-black/50 text-white focus:outline-none" />
                </li>
                <li v-for="(opt, i) in filteredCategories" :key="opt.id" role="option">
                    <div class="w-full text-left px-3 py-2 hover:bg-white/10 cursor-pointer" :class="{ 'bg-white/10': highlighted.category === i }" @mouseenter="highlighted.category = i" @click="selectCategory(opt)">
                        {{ opt.category || opt.label }}
                    </div>
                </li>
            </ul>
        </div>
        <label class="text-sm font-medium">Outfit</label>
        <div class="relative select-none">
            <div class="w-full px-3 py-2 rounded-xl bg-black/70 text-white text-left cursor-pointer" @click="openDropdown('outfit')">
                <span class="truncate">{{ outfitLabel }}</span>
            </div>
            <ul v-if="open.outfit" class="absolute left-0 right-0 mt-1 max-h-40 overflow-auto rounded-xl border border-white/10 bg-black/70 text-white shadow-2xl z-[9999] no-scrollbar" role="listbox">
                <li class="p-2">
                    <input type="text" v-model="search.outfit" placeholder="Suchen..." class="w-full px-2 py-1 rounded bg-black/50 text-white focus:outline-none" />
                </li>
                <li v-for="(opt, i) in filteredOutfits" :key="opt.id" role="option">
                    <div class="w-full text-left px-3 py-2 hover:bg-white/10 cursor-pointer" :class="{ 'bg-white/10': highlighted.outfit === i }" @mouseenter="highlighted.outfit = i" @click="selectOutfit(opt)">
                        {{ opt.name || opt.label }}
                    </div>
                </li>
            </ul>
        </div>
    </div>
</aside>
</template>

<script>
export default {
    name: 'nEUP-Menu',
    props: {
        categories: {
            type: Array,
            default: () => []
        },
        initialCategoryId: {
            type: String,
            default: ''
        },
        initialOutfitId: {
            type: String,
            default: ''
        },
        title: {
            type: String,
            default: 'nEUP-Menu'
        }
    },
    data() {
        return {
            menuShow: false,
            fallbackData: [],
            categoryId: '',
            outfitId: '',
            open: {
                category: false,
                outfit: false
            },
            highlighted: {
                category: -1,
                outfit: -1
            },
            search: {
                category: '',
                outfit: ''
            }
        }
    },
    computed: {
        data() {
            return this.categories.length > 0 ? this.categories : this.fallbackData
        },
        currentCategory() {
            return this.data.find(c => c.id === this.categoryId) || this.data[0]
        },
        currentOutfits() {
            return this.currentCategory ?.outfits || []
        },
        categoryLabel() {
            const c = this.data.find(c => c.id === this.categoryId)
            return c ? (c.category || c.label) : 'Choose category'
        },
        outfitLabel() {
            const o = this.currentOutfits.find(o => o.id === this.outfitId)
            return o ? (o.name || o.label) : 'Choose outfit'
        },
        filteredCategories() {
            return this.data.filter(c => (c.category || c.label).toLowerCase().includes(this.search.category.toLowerCase()))
        },
        filteredOutfits() {
            return this.currentOutfits.filter(o => (o.name || o.label).toLowerCase().includes(this.search.outfit.toLowerCase()))
        }
    },
    watch: {
        categoryId(newVal) {
            const first = this.data.find(c => c.id === newVal) ?.outfits?.[0] ?.id
            if (first) this.outfitId = first
        }
    },
    mounted() {
        window.app = this;
        window.addEventListener("keydown", this.handleKey);
        window.addEventListener("message", (event) => {
            if (event.data.option === "showEupMenu") {
                this.showMenu(event.data.eupData);
            } else if (event.data.option === "hideEupMenu") {
                this.hideMenu();
            }
        });
        document.addEventListener('mousedown', this.onClickOutside)
    },
    beforeUnmount() {
        window.removeEventListener("keydown", this.handleKey);
        document.removeEventListener('mousedown', this.onClickOutside)
    },
    methods: {
        handleKey(event) {
            if (event.key === "Escape" && this.menuShow == true) {
                this.menuShow = false;
                fetch(`https://nEUPMenu/hideEupMenu`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            }
        },
        showMenu(eupData) {
            this.menuShow = true;
            this.categoryId = true;
            this.outfitId = true;
            this.loadFromJson(eupData);
        },
        hideMenu() {
            this.menuShow = false;
        },
        loadFromJson(jsonData) {
            const grouped = {}
            jsonData.forEach(item => {
                if (!grouped[item.category]) grouped[item.category] = []
                grouped[item.category].push(item)
            })
            this.fallbackData = Object.keys(grouped).map(cat => ({
                id: cat,
                category: cat,
                outfits: grouped[cat]
            }))
        },
        openDropdown(which) {
            for (const key in this.open) {
                this.open[key] = false
            }
            this.open[which] = true
        },
        closeAll() {
            this.open.category = false;
            this.open.outfit = false
        },
        onClickOutside(e) {
            if (!this.$el.contains(e.target)) this.closeAll()
        },
        selectCategory(opt) {
            this.categoryId = opt.id;
            this.closeAll();
            this.search.category = ''
            this.search.outfit = ''
        },
        selectOutfit(opt) {
            this.outfitId = opt.id;
            this.closeAll();
            this.search.outfit = ''
            this.search.category = ''
            fetch(`https://nEUPMenu/setEUPOutfit`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    outfitId: this.outfitId,
                })
            });
        }
    }
}
</script>

<style scoped>
:host {
    pointer-events: none;
    background-color: transparent;
}

aside>div,
.relative,
div,
ul {
    pointer-events: auto;
}

/* Scrollbalken ausblenden */
.no-scrollbar::-webkit-scrollbar {
    display: none;
}

.no-scrollbar {
    -ms-overflow-style: none;
    scrollbar-width: none;
}
</style>
