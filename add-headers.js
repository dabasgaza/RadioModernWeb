const fs = require('fs');
const path = require('path');

const root = 'D:\\MVC\\RadioModernWeb';

const arabicNames = {
  'Episode': 'الحلقة', 'Program': 'البرنامج', 'Guest': 'الضيف',
  'Correspondent': 'المراسل', 'Coverage': 'التغطية', 'Employee': 'الموظف',
  'StaffRole': 'المسمى الوظيفي', 'Role': 'الدور', 'Permission': 'الصلاحية',
  'User': 'المستخدم', 'Account': 'حساب المستخدم', 'Home': 'الصفحة الرئيسية',
  'System': 'النظام', 'Admin': 'الإدارة', 'PublishingLog': 'سجل النشر',
  'PublishingRecord': 'سجل النشر', 'PlatformPublish': 'نشر المنصة',
  'SocialMediaPublishingLog': 'سجل النشر الرقمي',
  'WebsitePublishingLog': 'سجل نشر الموقع',
  'SocialMediaPlatform': 'منصة التواصل', 'ExecutionLog': 'سجل التنفيذ',
  'DatabaseBackupLog': 'سجل النسخ الاحتياطي',
  'DatabaseDashboard': 'لوحة تحكم قاعدة البيانات',
  'ActiveEpisode': 'الحلقة النشطة', 'ActiveProgram': 'البرنامج النشط',
  'AllDtos': 'DTOs المتنوعة', 'CorrespondentCoverage': 'تغطية المراسل',
  'AuditLog': 'سجل التدقيق', 'EpisodeGuest': 'حلقة-ضيف',
  'EpisodeEmployee': 'حلقة-موظف', 'EpisodeCorrespondent': 'حلقة-مراسل',
  'RolePermission': 'دور-صلاحية',
  'SocialMediaPublishingLogPlatform': 'سجل-منصة',
  'Notification': 'الإشعارات', 'Error': 'الخطأ',
  'PagedList': 'القائمة المقسمة', 'Status': 'الحالة',
  'TempDataKeys': 'مفاتيح TempData',
  'FluentValidationActionFilter': 'التحقق من الصحة',
  'SessionCapture': 'جلسة المستخدم', 'LogContext': 'سياق التسجيل',
  'GlobalException': 'الاستثناءات', 'PermissionPolicies': 'سياسات الصلاحيات',
  'ClaimsPrincipal': 'المطالبات', 'BCryptPassword': 'كلمات المرور',
  'ApplicationUser': 'المستخدم', 'ApplicationManagers': 'مديري المصادقة',
  'ApplicationUserClaimsPrincipalFactory': 'هوية المستخدم',
  'CurrentUser': 'المستخدم الحالي', 'MessageService': 'الرسائل',
  'MvcMessage': 'رسائل MVC', 'EpisodeEditViewModel': 'تحرير الحلقة',
  'AuditInterceptor': 'التدقيق التلقائي',
  'DbQueryPerformance': 'أداء الاستعلامات', 'ConnectionString': 'سلسلة الاتصال',
  'SecureConfiguration': 'الإعدادات الآمنة', 'DbSeeder': 'بذر البيانات',
  'IPermissionService': 'خدمة الصلاحيات', 'AppPermissions': 'الصلاحيات النظامية',
  'CollectionSync': 'مزامنة المجموعات', 'ConcurrencyException': 'التزامن',
  'CurrentSession': 'جلسة المستخدم', 'EpisodeStatus': 'حالة الحلقة',
  'PagedResult': 'النتائج المقسمة', 'QueryableExtensions': 'الاستعلامات',
  'Result': 'نتيجة العملية', 'SecurePassword': 'كلمات المرور',
  'SecurityHelper': 'الصلاحيات', 'UserSession': 'جلسة المستخدم',
  'UserValidation': 'المستخدم', 'ValidationPipeline': 'التحقق من الصحة',
  'Enums': 'التعدادات', 'CoverageViewModels': 'التغطية',
  'EpisodeViewModels': 'الحلقات', 'AdminViewModels': 'الإدارة',
  'SystemViewModels': 'النظام', 'RoleConfiguration': 'الدور',
  'UserConfiguration': 'المستخدم',
  'BroadcastWorkflowDBContextFactory': 'مصنع السياق',
  'BroadcastWorkflowDBContext': 'قاعدة البيانات', 'DatabaseHealth': 'قاعدة البيانات',
  'IdentitySynchronizer': 'مزامنة الهوية', 'SequentialCollection': 'المجموعة التسلسلية',
  'TestBroadcastWorkflowDbContext': 'سياق قاعدة البيانات للاختبار',
  'DatabaseFixture': 'قاعدة البيانات للاختبار',
  'AssertExtensions': 'التأكيدات', 'TestDataFactory': 'بيانات الاختبار',
  'TestTelemetry': 'التليمتري', 'ValidValidator': 'المدقق الصالح',
  'LayerTests': 'طبقات المشروع', 'ValidatorTests': 'المدققات',
  'AuditLogService': 'سجل التدقيق', 'CachedLookupService': 'البيانات المخبأة',
  'AuthService': 'المصادقة', 'DatabaseBackupScheduler': 'النسخ الاحتياطي',
  'DatabaseManagementService': 'إدارة قاعدة البيانات',
  'SystemDiagnosticsService': 'تشخيص النظام',
  'ReportsService': 'التقارير', 'EpisodeService': 'الحلقات',
  'EpisodeService.Commands': 'أوامر الحلقات',
  'EpisodeService.Queries': 'استعلامات الحلقات',
  'CurrentSessionProvider': 'مزود الجلسة',
  'EpisodeStatusTransition': 'انتقال حالة الحلقة',
  'SecurePasswordGenerator': 'مولد كلمات المرور',
  'UserValidationHelper': 'التحقق من المستخدم',
  'NotificationService': 'الإشعارات',
  'MessageServiceInitializer': 'مهيئ الرسائل',
  'ConnectionStringProtector': 'حماية الاتصال',
  'Configuration': 'تهيئة',
  'RoleConfiguration': 'تهيئة الدور',
  'UserConfiguration': 'تهيئة المستخدم',
  'NotificationHub': 'Hub الإشعارات',
  'DatabaseHealthCheck': 'فحص صحة قاعدة البيانات',
  'ApplicationManagers': 'مديري التطبيق',
  'IdentitySynchronizer': 'مزامن الهوية',
  'HomeControllerTests': 'اختبارات الصفحة الرئيسية',
  'CoverageControllerTests': 'اختبارات التغطية',
  'EpisodesControllerTests': 'اختبارات الحلقات',
  'ProgramsControllerTests': 'اختبارات البرامج',
  'GuestsControllerTests': 'اختبارات الضيوف',
  'CorrespondentsControllerTests': 'اختبارات المراسلين',
  'PublishingControllerTests': 'اختبارات النشر',
  'StaffControllersTests': 'اختبارات الموظفين',
  'AdminControllersTests': 'اختبارات الإدارة',
  'HomeController': 'الصفحة الرئيسية',
  'EpisodesController': 'الحلقات',
  'ProgramsController': 'البرامج',
  'GuestsController': 'الضيوف',
  'CorrespondentsController': 'المراسلين',
  'CoverageController': 'التغطية',
  'PublishingController': 'النشر',
  'StaffControllers': 'الموظفين',
  'AdminControllers': 'الإدارة',
  'AccountController': 'الحسابات',
  'SystemControllers': 'النظام',
  'ProgramDtoValidator': 'التحقق من البرنامج',
  'GuestDtoValidator': 'التحقق من الضيف',
  'EpisodeDtoValidator': 'التحقق من الحلقة',
  'CorrespondentDtoValidator': 'التحقق من المراسل',
  'CoverageDtoValidator': 'التحقق من التغطية',
  'EmployeeDtoValidator': 'التحقق من الموظف',
  'SocialMediaPlatformDtoValidator': 'التحقق من المنصة',
  'SocialMediaPublishingLogDtoValidator': 'التحقق من سجل النشر',
  'PlatformPublishDtoValidator': 'التحقق من نشر المنصة',
  'StaffRoleDtoValidator': 'التحقق من المسمى الوظيفي',
  'UserDtoValidator': 'التحقق من المستخدم',
  'EpisodeEditViewModelBuilder': 'بناء ViewModel التحرير',
  'EpisodeViewModel': 'ViewModel الحلقة',
  'CoverageViewModel': 'ViewModel التغطية',
  'ErrorViewModel': 'ViewModel الخطأ',
  'AdminViewModel': 'ViewModel الإدارة',
  'SystemViewModel': 'ViewModel النظام',
  'PagedListViewModel': 'ViewModel القائمة',
};

function pascalToArabic(str) {
  if (!str) return '';
  const parts = str.split(/(?=[A-Z])/).filter(p => p);
  const known = {
    'All': 'الكل', 'ById': 'حسب المعرف', 'Paged': 'مقسم',
    'Async': '', 'List': 'قائمة', 'Item': 'عنصر', 'Model': 'نموذج',
    'Info': 'معلومات', 'Data': 'بيانات', 'Value': 'قيمة',
    'Name': 'الاسم', 'Description': 'الوصف', 'Key': 'مفتاح',
    'Count': 'العدد', 'Total': 'المجموع', 'Page': 'صفحة',
    'Result': 'النتيجة', 'Context': 'السياق', 'Query': 'استعلام',
    'Command': 'أمر', 'Config': 'إعدادات', 'Map': 'خريطة',
    'Status': 'الحالة', 'Date': 'التاريخ', 'Time': 'الوقت',
    'User': 'المستخدم', 'Users': 'المستخدمين', 'Role': 'الدور',
    'Roles': 'الأدوار', 'Permission': 'الصلاحية', 'Permissions': 'الصلاحيات',
    'AuditLog': 'سجل التدقيق', 'AuditLogs': 'سجلات التدقيق',
    'Guest': 'الضيف', 'Guests': 'الضيوف', 'Episode': 'الحلقة',
    'Episodes': 'الحلقات', 'Program': 'البرنامج', 'Programs': 'البرامج',
    'Correspondent': 'المراسل', 'Correspondents': 'المراسلين',
    'Coverage': 'التغطية', 'Employee': 'الموظف', 'Staff': 'الموظفين',
    'Home': 'الرئيسية', 'Account': 'الحساب', 'System': 'النظام',
    'Admin': 'الإدارة', 'Reports': 'التقارير',
    'Audit': 'التدقيق', 'Log': 'سجل', 'Logs': 'السجلات',
    'Session': 'الجلسة', 'Secure': 'آمن', 'Password': 'كلمة المرور',
    'Config': 'الإعدادات', 'Configuration': 'الإعدادات',
    'Notification': 'الإشعارات', 'Message': 'الرسالة',
    'Matrix': 'المصفوفة', 'Filter': 'الفلتر', 'Search': 'البحث',
  };
  const translated = parts.map(p => known[p] || p);
  return translated.join(' ');
}

function getArabicName(name) {
  if (!name) return '';
  if (arabicNames[name]) return arabicNames[name];
  const cleaned = name.replace(/(Dto|Validator|Service|Controller|Configuration|Tests|ViewModel|Builder)$/, '');
  if (arabicNames[cleaned]) return arabicNames[cleaned];
  // Strip trailing common English suffixes before PascalCase translation
  const stripped = name.replace(/(Controller|Service|Manager|Provider|Helper|Builder|Filter|Hub|Handler|Dto|Model|ViewModel|Configuration|Tests|Validator|Interceptor|Scheduler|Extension)$/, '');
  const pascal = pascalToArabic(stripped);
  if (pascal !== name && pascal !== stripped) return pascal;
  return '';
}

const verbMap = [
  ['GetAllPaged', 'استرجاع المقسم'], ['GetAllActive', 'استرجاع النشط'],
  ['GetAll', 'استرجاع الكل'], ['GetActive', 'استرجاع النشط'],
  ['GetPaged', 'استرجاع المقسم'], ['GetUpcoming', 'استرجاع القادم'],
  ['GetRecent', 'استرجاع الأحدث'], ['GetBy', 'استرجاع حسب'],
  ['Get', 'استرجاع'],
  ['Create', 'إنشاء'], ['Add', 'إضافة'],
  ['Update', 'تحديث'], ['Edit', 'تعديل'],
  ['Delete', 'حذف'], ['Remove', 'إزالة'],
  ['Save', 'حفظ'], ['Upsert', 'إضافة أو تحديث'],
  ['Find', 'بحث عن'], ['Search', 'بحث عن'],
  ['Build', 'بناء'], ['MapTo', 'تحويل إلى'],
  ['Map', 'تحويل'],
  ['Filter', 'تصفية'], ['Sort', 'ترتيب'],
  ['Paginate', 'تقسيم إلى صفحات'],
  ['Handle', 'معالجة'], ['Process', 'معالجة'],
  ['Execute', 'تنفيذ'], ['Run', 'تشغيل'],
  ['Start', 'بدء'], ['Stop', 'إيقاف'],
  ['Initialize', 'تهيئة'], ['Configure', 'إعداد'],
  ['Register', 'تسجيل'],
  ['Authenticate', 'مصادقة'], ['Login', 'تسجيل دخول'],
  ['Logout', 'تسجيل خروج'],
  ['Authorize', 'تفويض'], ['Has', 'التحقق من'],
  ['Send', 'إرسال'], ['Receive', 'استقبال'],
  ['Notify', 'إرسال إشعار'], ['NotifyUser', 'إشعار المستخدم'],
  ['Publish', 'نشر'], ['PublishTo', 'نشر إلى'],
  ['Generate', 'توليد'], ['Calculate', 'حساب'],
  ['Format', 'تنسيق'], ['Parse', 'تحليل'],
  ['Convert', 'تحويل'],
  ['Check', 'فحص'], ['Ensure', 'تأكيد'],
  ['Try', 'محاولة'], ['Cancel', 'إلغاء'],
  ['Toggle', 'تبديل'], ['Enable', 'تفعيل'], ['Disable', 'تعطيل'],
  ['Lock', 'قفل'], ['Unlock', 'فتح'],
  ['Assign', 'تعيين'], ['Unassign', 'إلغاء تعيين'],
  ['Approve', 'اعتماد'], ['Reject', 'رفض'],
  ['Submit', 'إرسال'], ['Confirm', 'تأكيد'], ['Reset', 'إعادة تعيين'],
  ['Restore', 'استعادة'], ['Backup', 'نسخ احتياطي'],
  ['Cleanup', 'تنظيف'], ['Dispose', 'تخلص من الموارد'],
  ['On', 'عند'],
  ['Index', 'عرض قائمة'], ['Details', 'عرض تفاصيل'],
  ['List', 'عرض القائمة'],
  ['DeleteConfirmed', 'تأكيد الحذف'],
  ['Validate', 'التحقق من صحة'],
  ['Sync', 'مزامنة'], ['Import', 'استيراد'],
  ['Export', 'تصدير'],
  ['Seed', 'بذر البيانات'],
  ['Migrate', 'ترحيل'], ['CanConnect', 'اختبار الاتصال'],
  ['RegisterPolicies', 'تسجيل السياسات'],
  ['Require', 'اشتراط'],
  ['Connect', 'اتصال'], ['Disconnect', 'فصل'],
  ['Write', 'كتابة'], ['Read', 'قراءة'],
  ['Log', 'تسجيل'], ['Track', 'تتبع'],
  ['Throw', 'رمي'],
];

function getMethodVerb(methodName) {
  if (!methodName) return '';
  for (const [prefix, verb] of verbMap) {
    if (methodName.startsWith(prefix)) return verb;
  }
  return '';
}

function getMethodNoun(methodName) {
  if (!methodName) return '';
  // Find the longest matching verb prefix
  let prefixLen = 0;
  for (const [prefix] of verbMap) {
    if (methodName.startsWith(prefix) && prefix.length > prefixLen) {
      prefixLen = prefix.length;
    }
  }
  const rest = methodName.substring(prefixLen);
  if (!rest) return '';
  const arabic = getArabicName(rest);
  if (arabic) return arabic;
  // Try pascal split
  return pascalToArabic(rest);
}

function makeFileHeader(filename) {
  const name = getArabicName(filename.replace(/\.cs$/, ''));
  return `// ============================================================\n// ${filename.replace('.cs','')} — ${name || filename.replace('.cs','')}\n// ============================================================\n// المسؤولية: تعريف ${name || filename.replace('.cs','')}.\n// ============================================================\n`;
}

function hasXmlComment(lines, idx) {
  for (let i = idx - 1; i >= Math.max(0, idx - 5); i--) {
    const t = lines[i].trim();
    if (t === '' || t.startsWith('[')) continue;
    if (t.includes('/// <summary>')) return true;
    break;
  }
  return false;
}

function findInsertionPoint(lines, declIdx) {
  let insertIdx = declIdx;
  for (let i = declIdx - 1; i >= Math.max(0, declIdx - 20); i--) {
    const t = lines[i].trim();
    if (t === '' || t === '{' || t === '}') { insertIdx = i + 1; break; }
    if (t.startsWith('[')) { insertIdx = i; continue; }
    if (t.startsWith('///')) { insertIdx = i; break; }
    insertIdx = i + 1;
    break;
  }
  return insertIdx;
}

function processFile(filePath) {
  try {
    const rel = path.relative(root, filePath);
    const parts = rel.split(/[\\/]/);
    const filename = parts.pop();

    let content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split(/\r?\n/);

    // --- Step 1: Add/replace file header ---
    let lastEq = -1;
    for (let i = 0; i < Math.min(lines.length, 15); i++) {
      if (/^\/\/ =+\r?$/.test(lines[i])) lastEq = i;
    }
    if (lastEq >= 0) {
      content = lines.slice(lastEq + 1).join('\n').trimStart();
    }
    content = makeFileHeader(filename) + content;

    // --- Step 2: Add /// <summary> ---
    let newLines = content.split(/\r?\n/);
    const inserts = [];

    let currentClass = '';

    const classPattern = /^\s*(public|internal|private|protected)?\s*(static|abstract|sealed|partial)?\s*(class|interface|struct|record)\s+(\w+)/;
    const methodPattern = /^\s*(public|private|internal|protected)\s+(static|async|virtual|override|abstract|sealed|new)?\s*((partial\s+)?[\w<>,\[\]\?\s]+)\s+(\w+)\s*\(/;
    // Groups: $1=access $2=modifier $3=returnType $4=partial? $5=methodName
    // Groups: $1=access $2=className $3={ or =>
    const ctorPattern = /^\s*(public|private|internal|protected)\s+(\w+)\s*\([^)]*\)\s*(\{|=>)/;

    for (let i = 0; i < newLines.length; i++) {
      const trimmed = newLines[i].trimLeft();
      if (trimmed.startsWith('///') || trimmed.startsWith('//') || trimmed.startsWith('#')) continue;

      let match = null;
      let declType = '';
      let declName = '';

      // Class declaration
      if (!match && classPattern.test(trimmed)) {
        match = trimmed.match(classPattern);
        declType = match[3];
        declName = match[4];
        currentClass = declName;
      }

      // Constructor
      if (!match && ctorPattern.test(trimmed)) {
        match = trimmed.match(ctorPattern);
        declType = 'constructor';
        declName = match[2];
      }

      // Method
      if (!match && methodPattern.test(trimmed)) {
        const parenIdx = trimmed.indexOf('(');
        const afterParen = parenIdx >= 0 ? trimmed.substring(parenIdx) : '';
        let depth = 0;
        let parenEnd = -1;
        for (let j = 0; j < afterParen.length; j++) {
          if (afterParen[j] === '(') depth++;
          if (afterParen[j] === ')') { depth--; if (depth === 0) { parenEnd = j; break; } }
        }
        if (parenEnd > 0) {
          const restOfLine = afterParen.substring(parenEnd + 1).trim();
          if (restOfLine === '' || restOfLine.startsWith('{') || restOfLine.startsWith('=>') ||
              restOfLine.startsWith('where') || restOfLine.startsWith(';')) {
            match = trimmed.match(methodPattern);
            declType = 'method';
            declName = match ? match[5] : '';
          }
        }
      }

      if (match && declName && !hasXmlComment(newLines, i)) {
        const insertIdx = findInsertionPoint(newLines, i);
        let summary = '';

        if (declType === 'class' || declType === 'interface' || declType === 'struct' || declType === 'record') {
          const name = getArabicName(declName) || declName;
          const typeName = { class: 'صنف', interface: 'واجهة', struct: 'هيكل', record: 'سجل' }[declType] || 'صنف';
          summary = `/// <summary>\n/// ${typeName} ${name}.\n/// </summary>`;
        } else if (declType === 'constructor') {
          const name = getArabicName(declName) || declName;
          summary = `/// <summary>\n/// تهيئة ${name}.\n/// </summary>`;
        } else {
          const verb = getMethodVerb(declName);
          const noun = getMethodNoun(declName);
          const className = getArabicName(currentClass) || currentClass;

          if (verb && noun) {
            summary = `/// <summary>\n/// ${verb} ${noun}.\n/// </summary>`;
          } else if (verb && className) {
            summary = `/// <summary>\n/// ${verb} ${className}.\n/// </summary>`;
          } else if (verb) {
            summary = `/// <summary>\n/// ${verb}.\n/// </summary>`;
          } else {
            const readable = pascalToArabic(declName) || declName;
            summary = `/// <summary>\n/// ${readable}.\n/// </summary>`;
          }
        }

        inserts.push({ index: insertIdx, comment: summary });
      }
    }

    inserts.sort((a, b) => b.index - a.index);
    for (const ins of inserts) {
      const indent = newLines[ins.index] ? newLines[ins.index].match(/^\s*/)[0] : '';
      const commented = ins.comment.split('\n').map(l => indent + l);
      newLines.splice(ins.index, 0, ...commented);
    }

    fs.writeFileSync(filePath, newLines.join('\n'), 'utf8');
    console.log(`OK: ${rel} (${inserts.length} comments)`);
  } catch (e) {
    console.log(`ERR: ${rel} - ${e.message}`);
  }
}

function walk(dir) {
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  for (const e of entries) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) {
      if (e.name === 'obj' || e.name === 'bin' || e.name === 'Migrations') continue;
      walk(p);
    } else if (e.name.endsWith('.cs') && !e.name.includes('Designer')) {
      processFile(p);
    }
  }
}

walk(root);
console.log('\nDone.');
