require("chromedriver");
const { expect } = require("chai");

/**
 * Selenium testovi za Bella Perfumes - JavaScript
 *
 * Instalacija:
 *   npm install selenium-webdriver jest
 *   npm install --save-dev @types/selenium-webdriver
 *
 * Pokretanje:
 *   npx jest bella.test.js --testTimeout=30000
 *   npx jest -t "Admin Login"   <- samo određena grupa
 */

const { Builder, By, until, Select } = require("selenium-webdriver");
const chrome = require("selenium-webdriver/chrome");

// ─── Konstante ────────────────────────────────────────────────────────────────

const CLIENT_URL = "https://bellaperfumes-s-iproject.vercel.app";
const ADMIN_URL = "https://bellaperfumes-s-iproject-dd56.vercel.app";

const TEST_EMAIL = "tarhanisdzemila@gmail.com";
const TEST_PASSWORD = "test";

const ADMIN_EMAIL = "admin@bella.com";
const ADMIN_PASSWORD = "admin123";

const WAIT = 60000; // ms

// ─── Helpers ─────────────────────────────────────────────────────────────────

function buildDriver() {
  const options = new chrome.Options();

  options.addArguments("--no-sandbox");
  options.addArguments("--disable-dev-shm-usage");
  options.addArguments("--window-size=1280,900");
  options.setPageLoadStrategy("eager");

  return new Builder()
    .forBrowser("chrome")
    .setChromeOptions(options)
    .build();
}

async function waitFor(driver, locator, timeout = WAIT) {
  return driver.wait(until.elementLocated(locator), timeout);
}

async function waitVisible(driver, locator, timeout = WAIT) {
  const el = await waitFor(driver, locator, timeout);
  await driver.wait(until.elementIsVisible(el), timeout);
  return el;
}

async function sleep(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

async function clientLogin(driver, email = TEST_EMAIL, password = TEST_PASSWORD) {
  await driver.get(CLIENT_URL);
  await sleep(2000);
  const signInBtn = await waitVisible(driver, By.xpath("//button[contains(text(),'Sign in')]"));
  await signInBtn.click();
  await (await waitFor(driver, By.xpath("//input[@placeholder='Email']"))).sendKeys(email);
  await (await waitFor(driver, By.xpath("//input[@placeholder='Password']"))).sendKeys(password);
  await (await waitFor(driver, By.xpath("//button[text()='Login']"))).click();
  await sleep(2000);
}

async function adminLogin(driver, email = ADMIN_EMAIL, password = ADMIN_PASSWORD) {
  await driver.get(ADMIN_URL);
  await sleep(2000);
  await (await waitFor(driver, By.xpath("//input[@type='email']"))).sendKeys(email);
  await (await waitFor(driver, By.xpath("//input[@type='password']"))).sendKeys(password);
  await (await waitFor(driver, By.xpath("//button[contains(text(),'Sign In')]"))).click();
  await sleep(2000);
}

async function acceptAlert(driver) {
  const alert = await driver.switchTo().alert();
  const text = await alert.getText();
  await alert.accept();
  return text;
}

// ══════════════════════════════════════════════════════════════════════════════
// CLIENT FRONTEND
// ══════════════════════════════════════════════════════════════════════════════

describe("Client - Homepage", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Početna stranica se učitava", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    const title = await driver.getTitle();
    expect(title.toLowerCase()).to.include("bella");
  });

  it("Hero sekcija je vidljiva", async () => {
    await driver.get(CLIENT_URL);
    const hero = await waitVisible(driver, By.xpath("//*[contains(text(),'Find Your Signature Scent')]"));
    expect(await hero.isDisplayed()).equal(true);
  });

  it("Navigacioni linkovi su prisutni", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    for (const link of ["Home", "Products", "Categories", "Contact"]) {
      const el = await waitFor(driver, By.xpath(`//*[contains(text(),'${link}')]`));
      expect(el).to.be.ok;
    }
  });

  it("Most Popular slider se prikazuje", async () => {
    await driver.get(CLIENT_URL);
    const slider = await waitVisible(driver, By.xpath("//*[contains(text(),'Most Popular')]"));
    expect(await slider.isDisplayed()).equal(true);
  });

  it("Footer prikazuje kontakt info", async () => {
    await driver.get(CLIENT_URL);
    await driver.executeScript("window.scrollTo(0, document.body.scrollHeight)");
    await sleep(1500);
    const footer = await waitFor(driver, By.xpath("//*[contains(text(),'GET IN TOUCH')]"));
    expect(footer).to.be.ok;
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Client - Search", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Pretraga po imenu prikazuje dropdown", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    const input = await waitFor(driver, By.xpath("//input[contains(@placeholder,'Search by')]"));
    await input.sendKeys("Tom");
    await sleep(1500);
    const dropdown = await waitVisible(driver, By.xpath("//ul[contains(@class,'overflow-y-auto')]"));
    expect(await dropdown.isDisplayed()).equal(true);
  });

  it("Nepostojeći termin prikazuje 'No results found'", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    const input = await waitFor(driver, By.xpath("//input[contains(@placeholder,'Search by')]"));
    await input.sendKeys("xyzabc999notexist");
    await sleep(1500);
    const noResults = await waitVisible(driver, By.xpath("//*[contains(text(),'No results found')]"));
    expect(await noResults.isDisplayed()).equal(true);
  });

  it("Promjena filtera na Brand", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    const selectEl = await waitFor(driver, By.xpath("//select"));
    const select = new Select(selectEl);
    await select.selectByValue("brand");
    const input = await waitFor(driver, By.xpath("//input[contains(@placeholder,'Search by')]"));
    await input.sendKeys("Chanel");
    await sleep(1000);
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Client - Autentifikacija", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Sign in otvara login popup", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    await (await waitVisible(driver, By.xpath("//button[contains(text(),'Sign in')]"))).click();
    const popup = await waitVisible(driver, By.xpath("//h2[text()='Login']"));
    expect(await popup.isDisplayed()).equal(true);
  });

  it("X zatvara login popup", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    await (await waitVisible(driver, By.xpath("//button[contains(text(),'Sign in')]"))).click();
    await waitFor(driver, By.xpath("//h2[text()='Login']"));
    await (await waitFor(driver, By.xpath("//button[text()='✕']"))).click();
    await sleep(1000);
    const popups = await driver.findElements(By.xpath("//h2[text()='Login']"));
    expect(popups.length).equal(0);
  });

  it("Switch na Sign Up form", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    await (await waitVisible(driver, By.xpath("//button[contains(text(),'Sign in')]"))).click();
    await waitFor(driver, By.xpath("//h2[text()='Login']"));
    await (await waitFor(driver, By.xpath("//span[contains(text(),'Sign up')]"))).click();
    const signupTitle = await waitVisible(driver, By.xpath("//h2[text()='Sign Up']"));
    expect(await signupTitle.isDisplayed()).equal(true);
    const usernameInput = await waitFor(driver, By.xpath("//input[@placeholder='Username']"));
    expect(usernameInput).to.be.ok;
  });

  it("Pogrešni kredencijali prikazuju grešku", async () => {
    await driver.get(CLIENT_URL);
    await sleep(2000);
    await (await waitVisible(driver, By.xpath("//button[contains(text(),'Sign in')]"))).click();
    await (await waitFor(driver, By.xpath("//input[@placeholder='Email']"))).sendKeys("wrong@email.com");
    await (await waitFor(driver, By.xpath("//input[@placeholder='Password']"))).sendKeys("wrongpassword");
    await (await waitFor(driver, By.xpath("//button[text()='Login']"))).click();
    await sleep(2000);
    const error = await waitVisible(driver, By.xpath("//p[contains(@class,'text-red')]"));
    expect(await error.isDisplayed()).equal(true);
  });

  it("Uspješan login skriva Sign in dugme", async () => {
    await clientLogin(driver);
    await sleep(1000);
    const signInBtns = await driver.findElements(By.xpath("//button[contains(text(),'Sign in')]"));
    expect(signInBtns.length).equal(0);
  });

  it("Logout vraća Sign in dugme", async () => {
    await clientLogin(driver);
    await sleep(1000);
    const userBtn = await waitFor(driver, By.xpath("//button[.//*[local-name()='svg']][last()]"));
    await userBtn.click();
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Sign out')]"))).click();
    await sleep(2000);
    const signIn = await waitVisible(driver, By.xpath("//button[contains(text(),'Sign in')]"));
    expect(await signIn.isDisplayed()).equal(true);
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Client - Proizvodi", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Proizvodi se učitavaju na homepage", async () => {
    await driver.get(CLIENT_URL);
    await sleep(3000);
    const products = await driver.findElements(By.xpath("//div[contains(@class,'shadow-md') and .//img]"));
    expect(products.length).to.be.greaterThan(0);
  });

  it("Klik na product otvara product page", async () => {
    await driver.get(CLIENT_URL);
    await sleep(3000);
    const card = await waitFor(driver, By.xpath("(//div[contains(@class,'cursor-pointer') and .//img])[1]"));
    await card.click();
    await sleep(2000);
    expect(await driver.getCurrentUrl()).to.include("/page/");
  });

  it("Product page ima Add to cart dugme", async () => {
    await driver.get(CLIENT_URL);
    await sleep(3000);
    await (await waitFor(driver, By.xpath("(//div[contains(@class,'cursor-pointer') and .//img])[1]"))).click();
    await sleep(2000);
    const btn = await waitVisible(driver, By.xpath("//button[contains(text(),'Add to cart')]"));
    expect(await btn.isDisplayed()).equal(true);
  });

  it("Product page prikazuje Customer Reviews sekciju", async () => {
    await driver.get(CLIENT_URL);
    await sleep(3000);
    await (await waitFor(driver, By.xpath("(//div[contains(@class,'cursor-pointer') and .//img])[1]"))).click();
    await sleep(2000);
    const reviews = await waitVisible(driver, By.xpath("//*[contains(text(),'Customer Reviews')]"));
    expect(await reviews.isDisplayed()).equal(true);
  });

  it("Add to cart bez logina prikazuje alert", async () => {
    await driver.get(CLIENT_URL);
    await sleep(3000);
    await (await waitFor(driver, By.xpath("(//div[contains(@class,'cursor-pointer') and .//img])[1]"))).click();
    await sleep(2000);
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Add to cart')]"))).click();
    await sleep(1000);
    const alertText = await acceptAlert(driver);
    expect(alertText.toLowerCase()).to.include("logged in");
  });

  it("Add to cart ulogovanog korisnika radi", async () => {
    await clientLogin(driver);
    await sleep(2000);
    await (await waitFor(driver, By.xpath("(//div[contains(@class,'cursor-pointer') and .//img])[1]"))).click();
    await sleep(2000);
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Add to cart')]"))).click();
    await sleep(1000);
    await acceptAlert(driver);
  });

  it("Favorite bez logina prikazuje alert", async () => {
    await driver.get(CLIENT_URL);
    await sleep(3000);
    const favBtn = await waitFor(driver, By.xpath("(//button[contains(@class,'rounded-full') and contains(@class,'shadow')])[1]"));
    await favBtn.click();
    await sleep(1000);
    const alertText = await acceptAlert(driver);
    expect(alertText.toLowerCase()).to.include("logged in");
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Client - Korpa", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Korpa zahtijeva login", async () => {
    await driver.get(`${CLIENT_URL}/cart`);
    await sleep(2000);
    const src = await driver.getPageSource();
    expect(src.toLowerCase()).to.include("log in");
  });

  it("Korpa se učitava nakon logina", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/cart`);
    await sleep(2000);
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'Your Cart')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Proceed to Checkout dugme je vidljivo", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/cart`);
    await sleep(2000);
    const btn = await waitVisible(driver, By.xpath("//button[contains(text(),'PROCEED TO CHECKOUT')]"));
    expect(await btn.isDisplayed()).equal(true);
  });

  it("Checkout vodi na /order", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/cart`);
    await sleep(2000);
    await (await waitFor(driver, By.xpath("//button[contains(text(),'PROCEED TO CHECKOUT')]"))).click();
    await sleep(2000);
    expect(await driver.getCurrentUrl()).to.include("/order");
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Client - Narudžba", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Forma za dostavu ima sva polja", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/order`);
    await sleep(2000);
    for (const placeholder of ["First Name", "Last Name", "Email", "Phone", "Address - Street/City"]) {
      const el = await waitFor(driver, By.xpath(`//input[@placeholder='${placeholder}']`));
      expect(el).to.be.ok;
    }
  });

  it("Nevažeći promo kod prikazuje grešku", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/order`);
    await sleep(2000);
    await (await waitFor(driver, By.xpath("//input[@placeholder='Promo Code']"))).sendKeys("INVALIDCODE");
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Apply Code')]"))).click();
    await sleep(1000);
    const msg = await waitVisible(driver, By.xpath("//*[contains(text(),'Invalid promo code')]"));
    expect(await msg.isDisplayed()).equal(true);
  });

  it("Važeći promo kod DISCOUNT10 prikazuje success", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/order`);
    await sleep(2000);
    await (await waitFor(driver, By.xpath("//input[@placeholder='Promo Code']"))).sendKeys("DISCOUNT10");
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Apply Code')]"))).click();
    await sleep(1000);
    const msg = await waitVisible(driver, By.xpath("//*[contains(text(),'Promo code applied')]"));
    expect(await msg.isDisplayed()).equal(true);
  });

  it("Order Summary sekcija je vidljiva", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/order`);
    await sleep(2000);
    const summary = await waitVisible(driver, By.xpath("//*[contains(text(),'Order Summary')]"));
    expect(await summary.isDisplayed()).equal(true);
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Client - My Orders", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Orders stranica zahtijeva login", async () => {
    await driver.get(`${CLIENT_URL}/orders`);
    await sleep(2000);
    const src = await driver.getPageSource();
    expect(src.toLowerCase()).to.include("logged in");
  });

  it("Orders stranica se učitava", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/orders`);
    await sleep(3000);
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'My Orders')]"));
    expect(await title.isDisplayed()).equal(true);
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Client - Account", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Account prikazuje korisničke podatke", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/account`);
    await sleep(2000);
    expect(await waitFor(driver, By.xpath("//*[contains(text(),'Username')]"))).to.be.ok;
    expect(await waitFor(driver, By.xpath("//*[contains(text(),'Email')]"))).to.be.ok;
  });

  it("Edit dugme otvara edit mode", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/account`);
    await sleep(2000);
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Edit')]"))).click();
    await sleep(1000);
    const saveBtn = await waitVisible(driver, By.xpath("//button[contains(text(),'Save')]"));
    expect(await saveBtn.isDisplayed()).equal(true);
  });

  it("Cancel zatvara edit mode", async () => {
    await clientLogin(driver);
    await driver.get(`${CLIENT_URL}/account`);
    await sleep(2000);
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Edit')]"))).click();
    await sleep(1000);
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Cancel')]"))).click();
    await sleep(1000);
    const editBtn = await waitVisible(driver, By.xpath("//button[contains(text(),'Edit')]"));
    expect(await editBtn.isDisplayed()).equal(true);
  });
});

// ══════════════════════════════════════════════════════════════════════════════
// ADMIN PANEL
// ══════════════════════════════════════════════════════════════════════════════

describe("Admin - Login", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); });
  afterEach(async () => { await driver.quit(); });

  it("Admin login stranica se učitava", async () => {
    await driver.get(ADMIN_URL);
    const emailInput = await waitVisible(driver, By.xpath("//input[@type='email']"));
    expect(await emailInput.isDisplayed()).equal(true);
    const pageTitle = await driver.getTitle();
    expect(pageTitle.toLowerCase()).to.include("admin");
  });

  it("Pogrešni kredencijali prikazuju alert", async () => {
    await driver.get(ADMIN_URL);
    await (await waitFor(driver, By.xpath("//input[@type='email']"))).sendKeys("wrong@test.com");
    await (await waitFor(driver, By.xpath("//input[@type='password']"))).sendKeys("wrong");
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Sign In')]"))).click();
    const alertText = await acceptAlert(driver);
    expect(alertText).to.be.ok;
  });

  it("Uspješan login vodi na dashboard", async () => {
    await adminLogin(driver);
    const url = await driver.getCurrentUrl();
    const src = await driver.getPageSource();

    // Proveravamo da li je dashboard učitan (Add New Product)
    expect(url.includes("/add") || src.includes("Add New Product")).equal(true);
  });

  it("Logout vraća na login stranicu", async () => {
    await adminLogin(driver);
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Logout')]"))).click();

    const emailInput = await waitVisible(driver, By.xpath("//input[@type='email']"));
    expect(await emailInput.isDisplayed()).equal(true);
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Admin - Sidebar navigacija", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); await adminLogin(driver); });
  afterEach(async () => { await driver.quit(); });

  it("Sidebar ima sve stavke", async () => {
    for (const item of ["Add Product", "Products", "Orders", "Users", "Reviews"]) {
      const el = await waitFor(driver, By.xpath(`//*[contains(text(),'${item}')]`));
      expect(el).to.be.ok;
    }
  });

  it("Navigacija na Products listu", async () => {
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/list')]"))).click();
    await sleep(2000);
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Products')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Navigacija na Orders", async () => {
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/orders')]"))).click();
    await sleep(2000);
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Orders')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Navigacija na Users", async () => {
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/users')]"))).click();
    await sleep(2000);
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Users')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Navigacija na Reviews", async () => {
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/reviews')]"))).click();
    await sleep(2000);
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Reviews')]"));
    expect(await title.isDisplayed()).equal(true);
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Admin - Add Product", () => {
  let driver;
  beforeEach(async () => { driver = buildDriver(); await adminLogin(driver); });
  afterEach(async () => { await driver.quit(); });

  it("Forma ima sva potrebna polja", async () => {
    for (const name of ["name", "price", "stock", "discount", "size"]) {
      const el = await waitFor(driver, By.xpath(`//input[@name='${name}']`));
      expect(el).to.be.ok;
    }
    expect(await waitFor(driver, By.xpath("//textarea[@name='description']"))).to.be.ok;
    expect(await waitFor(driver, By.xpath("//select[@name='brandId']"))).to.be.ok;
    expect(await waitFor(driver, By.xpath("//select[@name='genderId']"))).to.be.ok;
  });

  it("Submit prazne forme prikazuje validacijski alert", async () => {
    await (await waitFor(driver, By.xpath("//button[contains(text(),'Add Product')]"))).click();
    await sleep(1000);
    const alertText = await acceptAlert(driver);
    expect(alertText).to.be.ok;
  });

  it("+ Add new brand toggle otvara input", async () => {
    await (await waitFor(driver, By.xpath("//*[contains(text(),'Add new brand')]"))).click();
    await sleep(500);
    const brandInput = await waitVisible(driver, By.xpath("//input[@placeholder='Brand name']"));
    expect(await brandInput.isDisplayed()).equal(true);
  });

  it("Gender select ima Female, Male, Unisex", async () => {
    const selectEl = await waitFor(driver, By.xpath("//select[@name='genderId']"));
    const select = new Select(selectEl);
    const options = await select.getOptions();
    const texts = await Promise.all(options.map((o) => o.getText()));
    expect(texts).to.include("Female");
    expect(texts).to.include("Male");
    expect(texts).to.include("Unisex");
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Admin - Products lista", () => {
  let driver;
  beforeEach(async () => {
    driver = buildDriver();
    await adminLogin(driver);
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/list')]"))).click();
    await sleep(3000);
  });
  afterEach(async () => { await driver.quit(); });

  it("Lista se učitava", async () => {
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Products')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Header kolone su vidljive", async () => {
    for (const col of ["Image", "Name", "Price", "Stock", "Discount"]) {
      expect(await waitFor(driver, By.xpath(`//*[contains(text(),'${col}')]`))).to.be.ok;
    }
  });

  it("Search input je prisutan", async () => {
    const search = await waitFor(driver, By.xpath("//input[@placeholder='Search by name...']"));
    expect(search).to.be.ok;
  });

  it("Edit dugme je prisutno", async () => {
    const editBtns = await driver.findElements(By.xpath("//button[contains(text(),'Edit')]"));
    expect(editBtns.length).to.be.greaterThan(0);
  });

  it("Klik na Edit otvara edit formu", async () => {
    await (await waitFor(driver, By.xpath("(//button[contains(text(),'Edit')])[1]"))).click();
    await sleep(1000);
    const editForm = await waitVisible(driver, By.xpath("//*[contains(text(),'Edit Product')]"));
    expect(await editForm.isDisplayed()).equal(true);
  });

  it("Delete dugme je prisutno", async () => {
    const deleteBtns = await driver.findElements(By.xpath("//button[contains(text(),'Delete')]"));
    expect(deleteBtns.length).to.be.greaterThan(0);
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Admin - Orders", () => {
  let driver;
  beforeEach(async () => {
    driver = buildDriver();
    await adminLogin(driver);
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/orders')]"))).click();
    await sleep(3000);
  });
  afterEach(async () => { await driver.quit(); });

  it("Orders stranica se učitava", async () => {
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Orders')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Status dropdown ima sve opcije", async () => {
    const selects = await driver.findElements(By.xpath("//select"));
    if (selects.length > 0) {
      const select = new Select(selects[0]);
      const options = await select.getOptions();
      const texts = await Promise.all(options.map((o) => o.getText()));
      expect(texts).to.include("Pending");
      expect(texts).to.include("Out for delivery");
      expect(texts).to.include("Delivered");
    }
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Admin - Users", () => {
  let driver;
  beforeEach(async () => {
    driver = buildDriver();
    await adminLogin(driver);
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/users')]"))).click();
    await sleep(3000);
  });
  afterEach(async () => { await driver.quit(); });

  it("Users stranica se učitava", async () => {
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Users')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Tabela ima sve kolone", async () => {
    for (const col of ["Username", "Email", "Status", "Verified"]) {
      expect(await waitFor(driver, By.xpath(`//*[contains(text(),'${col}')]`))).to.be.ok;
    }
  });

  it("Search input je prisutan", async () => {
    const search = await waitFor(driver, By.xpath("//input[@placeholder='Search by username...']"));
    expect(search).to.be.ok;
  });
});

// ─────────────────────────────────────────────────────────────────────────────

describe("Admin - Reviews", () => {
  let driver;
  beforeEach(async () => {
    driver = buildDriver();
    await adminLogin(driver);
    await (await waitFor(driver, By.xpath("//a[contains(@href,'/reviews')]"))).click();
    await sleep(3000);
  });
  afterEach(async () => { await driver.quit(); });

  it("Reviews stranica se učitava", async () => {
    const title = await waitVisible(driver, By.xpath("//*[contains(text(),'All Reviews')]"));
    expect(await title.isDisplayed()).equal(true);
  });

  it("Tabela ima sve kolone", async () => {
    for (const col of ["Username", "Product Name", "Review Text"]) {
      expect(await waitFor(driver, By.xpath(`//*[contains(text(),'${col}')]`))).to.be.ok;
    }
  });

  it("Search input je prisutan", async () => {
    const search = await waitFor(
      driver,
      By.xpath("//input[@placeholder='Search by username or review text...']")
    );
    expect(search).to.be.ok;
  });
});
